using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Shared.Presentation.Endpoints;
using Rentar.Shared.Presentation.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rentar.Modules.Rentals.Application.Common.Models;
using Xunit;

namespace Rentar.Api.IntegrationTests;

public class RentalsEndpointsTests(RentarApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly DateTime RentDate = new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime ReturnDate = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task Add_StoresLocalDateTimes_AndGetAllReturnsThem()
    {
        var carId = await AddCarAsync();
        var customerId = await AddCustomerAsync(await RegisterUserAsync("Sezer", "renter@example.com"));

        var response = await Client.PostJsonAsync("/api/v1/rentals/add", NewRental(carId, customerId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rentals = await Client.GetJsonAsync("/api/v1/rentals/getall");
        var listed = await rentals.ReadAsAsync<V1Response<List<RentalBody>>>();

        var rental = Assert.Single(listed.Data);
        Assert.Equal(RentDate, rental.RentDate);
        Assert.Equal(ReturnDate, rental.ReturnDate);
        Assert.Equal(carId, rental.CarId);
    }

    [Fact]
    public async Task GetRentalByCarId_ReturnsRentalOfThatCar()
    {
        var firstCar = await AddCarAsync(description: "Corolla 1.6");
        var secondCar = await AddCarAsync(description: "Yaris 1.5");
        var first = await AddCustomerAsync(await RegisterUserAsync("First", "first@example.com"));
        var second = await AddCustomerAsync(await RegisterUserAsync("Second", "second@example.com"));
        await Client.PostJsonAsync("/api/v1/rentals/add", NewRental(firstCar, first));
        await Client.PostJsonAsync("/api/v1/rentals/add", NewRental(secondCar, second));

        var response = await Client.GetJsonAsync($"/api/v1/rentals/getrentalbycarid?carId={secondCar}");
        var result = await response.ReadAsAsync<V1Response<RentalBody>>();

        Assert.Equal(secondCar, result.Data.CarId);
        Assert.Equal(second, result.Data.CustomerId);
    }

    [Fact]
    public async Task Add_WithoutDates_ReturnsValidationErrors()
    {
        var response = await Client.PostJsonAsync("/api/v1/rentals/add", new RentalBody
        {
            CarId = 1,
            CustomerId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.ReadAsAsync<ValidationErrorDetails>();
        Assert.Contains(error.Errors, e => e.PropertyName == "RentDate");
        Assert.Contains(error.Errors, e => e.PropertyName == "ReturnDate");
    }

    [Fact]
    public async Task GetRentalDetails_CombinesCustomerUserAndCarFromTheirModules()
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(12)));

        var userId = await RegisterUserAsync("Sezer", "renter@example.com");
        var customerId = await AddCustomerAsync(userId);
        var carId = await AddCarAsync();

        await Client.PostJsonAsync("/api/v1/rentals/add", NewRental(carId, customerId));
        // Fleet does not know this car, so its rental has no details to show.
        await Factory.StoreRentalOfMissingCarAsync(carId + 1, customerId, RentDate, ReturnDate);

        var response = await Client.GetJsonAsync("/api/v1/rentals/getrentaldetails");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var details = await response.ReadAsAsync<V1Response<List<RentalDetailResponse>>>();
        var detail = Assert.Single(details.Data);
        Assert.Equal(carId, detail.CarId);
        Assert.Equal("Toyota", detail.BrandName);
        Assert.Equal("Red", detail.ColorName);
        Assert.Equal("Corolla 1.6", detail.Description);
        Assert.Equal(950m, detail.DailyPrice);
        // The customer rents for no company.
        Assert.Null(detail.CompanyName);
        Assert.Equal("Sezer", detail.UserName);
        Assert.Equal(RentDate, detail.RentDate);
    }

    private async Task<int> RegisterUserAsync(string firstName, string email)
    {
        await Client.PostJsonAsync("/api/v1/auth/register", new RegisterUserCommand(firstName, "Tetik", email, "Passw0rd!"));

        var response = await Client.GetJsonAsync($"/api/v1/users/getbymail?email={email}");
        return (await response.ReadAsAsync<V1Response<UserBody>>()).Data.Id;
    }

    private async Task<int> AddCustomerAsync(int userId)
    {
        await Client.PostJsonAsync("/api/v1/customers/add", new CustomerBody { UserId = userId });

        var response = await Client.GetJsonAsync($"/api/v1/customers/getcustomersbyuserid?userId={userId}");
        return (await response.ReadAsAsync<V1Response<List<CustomerBody>>>()).Data.Single().Id;
    }

    /// <summary>Adds a car with a brand and a color of its own and answers its id; the newest records have the highest ids.</summary>
    private async Task<int> AddCarAsync(string brandName = "Toyota", string colorName = "Red", string description = "Corolla 1.6")
    {
        await Client.PostJsonAsync("/api/v1/brands/add", new BrandBody { BrandName = brandName });
        await Client.PostJsonAsync("/api/v1/colors/add", new ColorBody { ColorName = colorName });

        var brands = await (await Client.GetJsonAsync("/api/v1/brands/getall")).ReadAsAsync<V1Response<List<BrandBody>>>();
        var colors = await (await Client.GetJsonAsync("/api/v1/colors/getall")).ReadAsAsync<V1Response<List<ColorBody>>>();

        await Client.PostJsonAsync("/api/v1/cars/add", new CarBody
        {
            BrandId = brands.Data.Max(b => b.Id),
            ColorId = colors.Data.Max(c => c.Id),
            ModelYear = 2021,
            DailyPrice = 950m,
            Description = description,
            MinimumFindeksRating = 700
        });

        var cars = await (await Client.GetJsonAsync("/api/v1/cars/getall")).ReadAsAsync<V1Response<List<CarBody>>>();
        return cars.Data.Max(c => c.Id);
    }

    private static RentalBody NewRental(int carId = 1, int customerId = 1)
    {
        return new RentalBody
        {
            CarId = carId,
            CustomerId = customerId,
            RentDate = RentDate,
            ReturnDate = ReturnDate
        };
    }
}
