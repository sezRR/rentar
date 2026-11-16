using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
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
        var rental = await response.ReadAsAsync<Rental>();
        Assert.Equal($"/api/v2/rentals/{rental.Id}", response.Headers.Location?.OriginalString);

        var fetched = await (await Client.GetJsonAsync($"/api/v2/rentals/{rental.Id}")).ReadAsAsync<Rental>();
        Assert.Equal(car.Id, fetched.CarId);
        Assert.Equal(customer.Id, fetched.CustomerId);
        Assert.Equal(RentDate, fetched.RentDate);
        Assert.Equal(ReturnDate, fetched.ReturnDate);
    }

    [Fact]
    public async Task Create_ForACarThatIsNotStored_IsABadRequest()
    {
        var customer = await CreateCustomerAsync();

        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(404, customer.Id, RentDate, ReturnDate));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CarNotFound);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<Rental>>());
    }

    [Fact]
    public async Task Create_ForACustomerThatIsNotStored_IsABadRequest()
    {
        var car = await CreateCarAsync();

        var response = await Client.PostJsonAsync("/api/v2/rentals", new CreateRentalCommand(car.Id, 404, RentDate, ReturnDate));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CustomerNotFound);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<Rental>>());
    }

    [Fact]
    public async Task GetAll_FiltersByCar_AndReturnsEveryRentalOfIt()
    {
        var brand = await CreateBrandAsync();
        var color = await CreateColorAsync();
        var firstCar = await CreateCarAsync(brand.Id, color.Id, "Corolla 1.6");
        var secondCar = await CreateCarAsync(brand.Id, color.Id, "Yaris 1.5");
        var first = await CreateCustomerAsync("first@example.com");
        var second = await CreateCustomerAsync("second@example.com");

        // v1's getrentalbycarid expects at most one rental per car; the v2 filter lists them all.
        await CreateRentalAsync(firstCar.Id, first.Id);
        await CreateRentalAsync(secondCar.Id, first.Id, RentDate.AddDays(10), ReturnDate.AddDays(10));
        await CreateRentalAsync(secondCar.Id, second.Id, RentDate.AddDays(20), ReturnDate.AddDays(20));

        var all = await (await Client.GetJsonAsync("/api/v2/rentals")).ReadAsAsync<List<Rental>>();
        var ofCar = await (await Client.GetJsonAsync($"/api/v2/rentals?carId={secondCar.Id}")).ReadAsAsync<List<Rental>>();

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
    public async Task Details_CombineCustomerUserAndCarFromTheirModules()
    {
        FreezeClockAtNoon();
        var customer = await CreateCustomerAsync("renter@example.com", "Rentar Logistics");
        var car = await CreateCarAsync("Toyota", "Red", "Corolla 1.6");
        var deletedCar = await CreateCarAsync("Fiat", "Blue", "Egea 1.4");
        await CreateRentalAsync(car.Id, customer.Id);
        await CreateRentalAsync(deletedCar.Id, customer.Id);
        // Fleet no longer knows this car, so its rental has no details to show.
        var delete = await Client.DeleteAsync($"/api/v2/cars/{deletedCar.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

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
    public async Task Details_DuringTheMaintenanceHour_AreServiceUnavailable()
    {
        FreezeClockInMaintenanceHour();

        var response = await Client.GetJsonAsync("/api/v2/rentals/details");

        await AssertProblemAsync(response, HttpStatusCode.ServiceUnavailable, Messages.MaintenanceTime);
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

    private Task<Rental> CreateRentalAsync(int carId, int customerId, DateTime? rentDate = null, DateTime? returnDate = null)
    {
        return CreateAsync<Rental>("/api/v2/rentals", new CreateRentalCommand(carId, customerId, rentDate ?? RentDate, returnDate ?? ReturnDate));
    }

    [Fact]
    public async Task GetByCompany_ListsOnlyTheRentalsOfTheCompanysCustomers()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var companyCustomer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(owner.Id, CompanyId: company.Id));
        var privateCustomer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(owner.Id, "Sezer Tetik"));
        var car = await CreateCarAsync();
        var companyRental = await CreateAsync<Rental>("/api/v2/rentals", new CreateRentalCommand(car.Id, companyCustomer.Id, RentDate, ReturnDate));
        await CreateAsync<Rental>("/api/v2/rentals", new CreateRentalCommand(car.Id, privateCustomer.Id, RentDate.AddDays(5), ReturnDate.AddDays(5)));

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/rentals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rentals = await response.ReadAsAsync<List<Rental>>();
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
