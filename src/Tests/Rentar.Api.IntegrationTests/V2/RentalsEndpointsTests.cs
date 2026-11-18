using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Rentals.Application;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Domain;
using Rentar.Modules.Rentals.Presentation.Endpoints.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class RentalsEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    private static readonly DateTime RentDate = new(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime ReturnDate = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task Create_AnswersCreated_WithTheRentalAndWhereItLives()
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();

        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(car.Id, customer.Id, RentDate, ReturnDate));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var rental = await response.ReadAsAsync<RentalBody>();
        Assert.Equal($"/api/v2/rentals/{rental.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal(RentalStatus.Pending, rental.Status);

        var fetched = await (await Client.GetJsonAsync($"/api/v2/rentals/{rental.Id}")).ReadAsAsync<RentalBody>();
        Assert.Equal(car.Id, fetched.CarId);
        Assert.Equal(customer.Id, fetched.CustomerId);
        Assert.Equal(RentDate, fetched.RentDate);
        Assert.Equal(ReturnDate, fetched.ReturnDate);
    }

    [Fact]
    public async Task Create_ThenThePaymentGoesThrough_ConfirmsTheRental()
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var rental = await CreateRentalAsync(car.Id, customer.Id);

        await DeliverEventsAsync();

        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(rental.Id)).Status);
        Assert.Equal(1, Factory.PaymentGateway.Charges);
    }

    [Fact]
    public async Task Create_WhilePending_HoldsTheCar()
    {
        var car = await CreateCarAsync();
        await CreateRentalAsync(car.Id, (await CreateCustomerAsync()).Id);

        var availability = await (await Client.GetJsonAsync(
            $"/api/v2/rentals/availability?carId={car.Id}&rentDate=2026-09-18T10:00:00&returnDate=2026-09-20T10:00:00")).ReadAsAsync<RentalAvailabilityResponse>();

        Assert.False(availability.Available);
    }

    [Fact]
    public async Task Create_ThenThePaymentIsDeclined_KeepsTheRentalCancelled_AndTheCarFree()
    {
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var rental = await CreateRentalAsync(car.Id, customer.Id);
        Factory.PaymentGateway.Declines = true;

        await DeliverEventsAsync();

        Assert.Equal(RentalStatus.Cancelled, (await RentalAsync(rental.Id)).Status);

        var availability = await (await Client.GetJsonAsync(
            $"/api/v2/rentals/availability?carId={car.Id}&rentDate=2026-09-18T10:00:00&returnDate=2026-09-20T10:00:00")).ReadAsAsync<RentalAvailabilityResponse>();
        Assert.True(availability.Available);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/rentals/details")).ReadAsAsync<List<RentalDetailResponse>>());
    }

    [Fact]
    public async Task Create_WhenTheConfirmationCanNotBeStored_ConfirmsOnARetry_WithoutPayingTwice()
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var rental = await CreateRentalAsync(car.Id, customer.Id);

        await using (await Factory.RefuseAsync("rentals", "Rentals", "UPDATE"))
        {
            await DeliverEventsAsync();
        }

        Assert.Equal(RentalStatus.Pending, (await RentalAsync(rental.Id)).Status);
        var outcome = Assert.Single(await Factory.OutboxAsync("payments"));
        Assert.Equal(("PaymentSucceededEvent", false, 1), (outcome.Event, outcome.Processed, outcome.Attempts));
        Assert.Contains("refused by test", outcome.Error);

        Factory.Clock.FreezeAt(DateTimeOffset.UtcNow.AddMinutes(1));
        await DeliverEventsAsync();

        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(rental.Id)).Status);
        Assert.Equal(1, Factory.PaymentGateway.Charges);
    }

    [Fact]
    public async Task Create_ForACarThatIsNotStored_IsABadRequest()
    {
        var customer = await CreateCustomerAsync();

        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(404, customer.Id, RentDate, ReturnDate));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CarNotFound);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<RentalBody>>());
    }

    [Fact]
    public async Task Create_ForACustomerThatIsNotStored_IsABadRequest()
    {
        var car = await CreateCarAsync();

        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(car.Id, 404, RentDate, ReturnDate));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CustomerNotFound);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<RentalBody>>());
    }

    [Fact]
    public async Task GetAll_FiltersByCar_AndReturnsEveryRentalOfIt()
    {
        var brand = await CreateBrandAsync();
        var color = await CreateColorAsync();
        var firstCar = await CreateCarAsync(brand.Id, color.Id);
        var secondCar = await CreateCarAsync(brand.Id, color.Id, "Yaris 1.5");
        var first = await CreateCustomerAsync("first@example.com");
        var second = await CreateCustomerAsync("second@example.com");

        // v1's getrentalbycarid expects at most one rental per car; the v2 filter lists them all.
        await CreateRentalAsync(firstCar.Id, first.Id);
        await CreateRentalAsync(secondCar.Id, first.Id, RentDate.AddDays(10), ReturnDate.AddDays(10));
        await CreateRentalAsync(secondCar.Id, second.Id, RentDate.AddDays(20), ReturnDate.AddDays(20));

        var all = await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<RentalBody>>();
        var ofCar = await (await Client.GetJsonAsync($"/api/v2/rentals?carId={secondCar.Id}")).ReadAsAsync<List<RentalBody>>();

        Assert.Equal(3, all.Count);
        Assert.Equal([first.Id, second.Id], ofCar.Select(r => r.CustomerId).Order());
        Assert.All(ofCar, r => Assert.Equal(secondCar.Id, r.CarId));
    }

    [Theory]
    [InlineData("2026-09-19T10:00:00", "2026-09-22T10:00:00", false)]
    [InlineData("2026-09-10T10:00:00", "2026-09-18T10:00:00", false)]
    [InlineData("2026-09-21T10:00:00", "2026-09-25T10:00:00", true)]
    public async Task Availability_IsFalseOnlyWhenTheCarIsRentedDuringThePeriod(string from, string to, bool expected)
    {
        var car = await CreateCarAsync();
        await CreateRentalAsync(car.Id, (await CreateCustomerAsync()).Id);

        var response = await Client.GetJsonAsync($"/api/v2/rentals/availability?carId={car.Id}&rentDate={from}&returnDate={to}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var availability = await response.ReadAsAsync<RentalAvailabilityResponse>();
        Assert.Equal(expected, availability.Available);
        Assert.Equal(car.Id, availability.CarId);
        Assert.Equal(DateTime.Parse(from), availability.RentDate);
    }

    [Fact]
    public async Task Availability_OfAnotherCar_IsNotAffected()
    {
        var car = await CreateCarAsync();
        await CreateRentalAsync(car.Id, (await CreateCustomerAsync()).Id);

        var response = await Client.GetJsonAsync($"/api/v2/rentals/availability?carId={car.Id + 1}&rentDate=2026-09-19T10:00:00&returnDate=2026-09-19T12:00:00");

        Assert.True((await response.ReadAsAsync<RentalAvailabilityResponse>()).Available);
    }

    [Fact]
    public async Task Details_CombineCustomerUserCompanyAndCarFromTheirModules()
    {
        // Signed in before the clock is frozen: the access token's times come from the clock.
        var owner = await SignInAsNewUserAsync("renter@example.com");
        var company = await RegisterCompanyAsync();
        var customer = await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(owner.Id, company.Id));
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        await CreateRentalAsync(car.Id, customer.Id);
        // Fleet does not know this car, so its rental has no details to show.
        await Factory.StoreRentalOfMissingCarAsync(car.Id + 1, customer.Id, RentDate, ReturnDate);

        var response = await Client.GetJsonAsync("/api/v2/rentals/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = Assert.Single(await response.ReadAsAsync<List<RentalDetailResponse>>());
        Assert.Equal(car.Id, detail.CarId);
        Assert.Equal("Toyota", detail.BrandName);
        Assert.Equal("Red", detail.ColorName);
        Assert.Equal("Rentar Logistics", detail.CompanyName);
        Assert.Equal("Sezer", detail.UserName);
    }

    [Fact]
    public async Task Create_WithoutDates_ReturnsValidationProblem()
    {
        var response = await Client.PostJsonAsync("/api/v2/rentals", new { carId = 1, customerId = 1 });

        await AssertValidationProblemAsync(response, "rentDate", "returnDate");
    }

    [Fact]
    public async Task Create_ReturningBeforeTheRentDate_ReturnsValidationProblem()
    {
        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(1, 3, ReturnDate, RentDate));

        await AssertValidationProblemAsync(response, "returnDate");
    }

    [Fact]
    public async Task Availability_ForAPeriodEndingBeforeItStarts_ReturnsValidationProblem()
    {
        var response = await Client.GetJsonAsync("/api/v2/rentals/availability?carId=1&rentDate=2026-09-20T10:00:00&returnDate=2026-09-18T10:00:00");

        await AssertValidationProblemAsync(response, "returnDate");
    }

    [Fact]
    public async Task UnknownRental_IsNotFound()
    {
        var response = await Client.GetJsonAsync("/api/v2/rentals/404");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Rental 404 was not found.");
    }

    private async Task<RentalBody> RentalAsync(int rentalId)
    {
        return await (await Client.GetJsonAsync($"/api/v2/rentals/{rentalId}")).ReadAsAsync<RentalBody>();
    }

    private Task<RentalBody> CreateRentalAsync(int carId, int customerId, DateTime? rentDate = null, DateTime? returnDate = null)
    {
        return CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(carId, customerId, rentDate ?? RentDate, returnDate ?? ReturnDate));
    }

    [Fact]
    public async Task GetByCompany_ListsOnlyTheRentalsOfTheCompanysCustomers()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var companyCustomer = await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(owner.Id, company.Id));
        var privateCustomer = await CreateCustomerAsync("private@example.com");
        var car = await CreateCarAsync();
        var companyRental = await CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(car.Id, companyCustomer.Id, RentDate, ReturnDate));
        await CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(car.Id, privateCustomer.Id, RentDate.AddDays(5), ReturnDate.AddDays(5)));

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/rentals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rentals = await response.ReadAsAsync<List<RentalBody>>();
        Assert.Equal([companyRental.Id], rentals.Select(r => r.Id));
    }

    [Fact]
    public async Task GetByCompany_ForAMemberWithoutRentalsRead_IsForbidden()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        await AddEmployeeAsync(company.Id, member.Id, "Member");
        await SignInAsync("member@example.com");

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/rentals");

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.PermissionMissing(CompanyPermissions.RentalsRead));
    }

    [Fact]
    public async Task GetByCompany_ForSomeoneWhoDoesNotWorkThere_IsNotFound()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        await SignInAsNewUserAsync("outsider@example.com");

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/rentals");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, Messages.CompanyNotFound(company.Id));
    }

    [Fact]
    public async Task GetByCompany_WithoutAnAccessToken_IsUnauthorized()
    {
        var response = await Client.GetJsonAsync("/api/v2/companies/1/rentals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
