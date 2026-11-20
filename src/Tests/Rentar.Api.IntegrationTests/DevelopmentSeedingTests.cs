using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rentar.Api.IntegrationTests.V2;
using Rentar.Modules.Fleet.Presentation.Endpoints.V2;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Presentation.Endpoints.V2;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests;

public class DevelopmentSeedingTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task DevelopmentStartup_SeedsRelatedDemoData_AndUsableAccounts()
    {
        await using var app = WithSeeding(migrate: true);
        using var client = app.CreateClient();

        var users = await (await client.GetJsonAsync("/api/v2/users")).ReadItemsAsync<UserResponse>();
        var cars = await (await client.GetJsonAsync("/api/v2/cars")).ReadItemsAsync<CarBody>();
        var customers = await (await client.GetJsonAsync("/api/v2/customers")).ReadItemsAsync<CustomerBody>();
        var rentals = await (await client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>();
        Assert.Equal(3, users.Count);
        Assert.Equal(4, cars.Count);
        Assert.Equal(2, customers.Count);
        Assert.Equal(2, rentals.Count);
        Assert.All(rentals, rental =>
        {
            Assert.Contains(rental.CarId, cars.Select(car => car.Id));
            Assert.Contains(rental.CustomerId, customers.Select(customer => customer.Id));
            Assert.Equal(RentalStatus.Confirmed, rental.Status);
            Assert.True(rental.ReturnDate > rental.RentDate);
        });

        foreach (var customer in customers)
        {
            Assert.Contains(customer.UserId, users.Select(user => user.Id));
            var score = await (await client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).ReadAsAsync<CustomerScoreBody>();
            Assert.InRange(score.FindeksRating, 1200, 1900);
        }

        foreach (var email in new[] { "owner@rentar.local", "member@rentar.local", "renter@rentar.local" })
        {
            var login = await client.PostJsonAsync("/api/v2/auth/tokens", new VerifyCredentialsQuery(email, Password));
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        var tokenResponse = await client.PostJsonAsync("/api/v2/auth/tokens", new VerifyCredentialsQuery("owner@rentar.local", Password));
        var token = await tokenResponse.ReadAsAsync<AccessToken>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        var company = Assert.Single(await (await client.GetJsonAsync("/api/v2/companies")).ReadItemsAsync<CompanyBody>());
        Assert.Equal("Rentar Demo", company.Name);
        Assert.Equal(company.Id, Assert.Single(customers, customer => customer.CompanyId != null).CompanyId);
        var employees = await client.GetJsonAsync($"/api/v2/companies/{company.Id}/employees");
        Assert.Equal(HttpStatusCode.OK, employees.StatusCode);
        Assert.Equal(2, (await employees.ReadItemsAsync<EmployeeBody>()).Count);
        await AssertNoEventsOrChargesAsync();
    }

    [Fact]
    public async Task RepeatedSeeding_DoesNotDuplicateData_ResetEdits_OrMoveRentalDates()
    {
        EnableSeeding();
        await SeedAsync();
        var user = Assert.Single(await (await Client.GetJsonAsync("/api/v2/users?email=renter@rentar.local")).ReadItemsAsync<UserResponse>());
        var car = (await (await Client.GetJsonAsync("/api/v2/cars")).ReadItemsAsync<CarBody>()).First();
        var rentals = await (await Client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>();
        var counts = await CountsAsync();
        Assert.Equal(HttpStatusCode.NoContent, (await Client.PutJsonAsync($"/api/v2/users/{user.Id}",
            new UserRequest("Edited", "Renter", user.Email, "N3wPassw0rd!"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.PutJsonAsync($"/api/v2/cars/{car.Id}",
            new CarRequest(car.BrandId, car.ColorId, car.ModelYear, 999m, car.Description, car.MinimumFindeksRating))).StatusCode);

        Factory.Clock.FreezeAt(DateTimeOffset.Now.AddMonths(1));
        await SeedAsync();
        await SeedAsync();

        Assert.Equal(counts.OrderBy(row => row.Key), (await CountsAsync()).OrderBy(row => row.Key));
        var editedUser = await (await Client.GetJsonAsync($"/api/v2/users/{user.Id}")).ReadAsAsync<UserResponse>();
        Assert.Equal("Edited", editedUser.FirstName);
        var login = await Client.PostJsonAsync("/api/v2/auth/tokens", new VerifyCredentialsQuery(user.Email, "N3wPassw0rd!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(999m, (await (await Client.GetJsonAsync($"/api/v2/cars/{car.Id}")).ReadAsAsync<CarBody>()).DailyPrice);
        var after = await (await Client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>();
        Assert.Equal(rentals.Select(rental => (rental.Id, rental.RentDate, rental.ReturnDate)).OrderBy(rental => rental.Id),
            after.Select(rental => (rental.Id, rental.RentDate, rental.ReturnDate)).OrderBy(rental => rental.Id));
        // The demo rentals keep the car, user and price they were seeded with, whatever was edited since.
        Assert.All(rentals, rental => Assert.NotEqual("", rental.Car.BrandName));
        Assert.Contains(rentals, rental => rental.Customer.FirstName == "Renter");
        Assert.Equal(rentals.Select(rental => (rental.Id, rental.Car, rental.Customer, rental.DailyPrice)).OrderBy(rental => rental.Id),
            after.Select(rental => (rental.Id, rental.Car, rental.Customer, rental.DailyPrice)).OrderBy(rental => rental.Id));
        await AssertNoEventsOrChargesAsync();
    }

    [Fact]
    public async Task Seeding_PreservesExistingRows_AndFillsInTheMissingDemoData()
    {
        var owner = await CreateUserAsync("owner@rentar.local", "Existing");
        var brand = await CreateBrandAsync("Toyota");
        var color = await CreateColorAsync("Red");
        var existingCar = await CreateCarAsync(brand.Id, color.Id, "My existing car");

        EnableSeeding();
        await SeedAsync();

        var users = await (await Client.GetJsonAsync("/api/v2/users")).ReadItemsAsync<UserResponse>();
        Assert.Equal(3, users.Count);
        Assert.Equal(owner.Id, Assert.Single(users, user => user.Email == owner.Email).Id);
        Assert.Equal("Existing", Assert.Single(users, user => user.Id == owner.Id).FirstName);
        var brands = await (await Client.GetJsonAsync("/api/v2/brands")).ReadItemsAsync<BrandBody>();
        Assert.Equal(brand.Id, Assert.Single(brands, row => row.BrandName == "Toyota").Id);
        var cars = await (await Client.GetJsonAsync("/api/v2/cars")).ReadItemsAsync<CarBody>();
        Assert.Equal(5, cars.Count);
        Assert.Equal("My existing car", Assert.Single(cars, car => car.Id == existingCar.Id).Description);
    }

    [Fact]
    public async Task Seeding_IsDisabledOutsideDevelopment()
    {
        // The environment is fixed when a host is built, so this test needs one of its own. Any environment other than
        // Development takes the same branch.
        await using var app = WithSeeding("Production");
        using var client = app.CreateClient();

        Assert.All(await CountsAsync(), row => Assert.Equal(0, row.Value));
        Assert.Equal(0, Factory.PaymentGateway.Charges);
    }

    [Fact]
    public async Task Seeding_CanBeOptedOut()
    {
        // The shared host is a Development host that has not opted in.
        await SeedAsync();

        Assert.All(await CountsAsync(), row => Assert.Equal(0, row.Value));
        Assert.Equal(0, Factory.PaymentGateway.Charges);
    }

    [Fact]
    public async Task AFailedSeed_RollsBackAllModules_AndCanBeRetried()
    {
        EnableSeeding();
        await using (await Factory.RefuseAsync("fleet", "Cars", "INSERT"))
        {
            await Assert.ThrowsAsync<DbUpdateException>(SeedAsync);
        }

        Assert.All(await CountsAsync(), row => Assert.Equal(0, row.Value));
        await SeedAsync();
        Assert.Equal(3, (await (await Client.GetJsonAsync("/api/v2/users")).ReadItemsAsync<UserResponse>()).Count);
        Assert.Equal(4, (await (await Client.GetJsonAsync("/api/v2/cars")).ReadItemsAsync<CarBody>()).Count);
    }

    [Fact]
    public async Task ConcurrentSeeding_SeedsOnlyOneCopyOfTheData()
    {
        // Each run opens its own scope, connection and transaction, as two hosts starting at once would, so the database
        // lock is what keeps them from seeding twice.
        EnableSeeding();

        await Task.WhenAll(
            Task.Run(SeedAsync, TestContext.Current.CancellationToken),
            Task.Run(SeedAsync, TestContext.Current.CancellationToken));

        Assert.Equal(3, (await (await Client.GetJsonAsync("/api/v2/users")).ReadItemsAsync<UserResponse>()).Count);
        Assert.Equal(4, (await (await Client.GetJsonAsync("/api/v2/cars")).ReadItemsAsync<CarBody>()).Count);
        Assert.Equal(2, (await (await Client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>()).Count);
        await AssertNoEventsOrChargesAsync();
    }

    /// <summary>Seeds the shared host the way Program does at startup; it seeds only once a test called <c>EnableSeeding()</c>.</summary>
    private Task SeedAsync()
    {
        return Factory.Services.SeedDevelopmentDataAsync(TestContext.Current.CancellationToken);
    }

    private WebApplicationFactory<Program> WithSeeding(string environment = "Development", bool enabled = true, bool migrate = false)
    {
        return Factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Database:SeedOnStartup"] = enabled.ToString(),
                ["Database:MigrateOnStartup"] = migrate.ToString()
            }));
        });
    }

    private async Task<Dictionary<string, long>> CountsAsync()
    {
        await using var connection = new NpgsqlConnection(Factory.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var counts = new Dictionary<string, long>();
        foreach (var table in Factory.Services.GetServices<IModuleDatabase>().SelectMany(database => database.GetTableNames()))
        {
            await using var command = new NpgsqlCommand($"SELECT COUNT(*) FROM {table}", connection);
            counts.Add(table, (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!);
        }

        return counts;
    }

    private async Task AssertNoEventsOrChargesAsync()
    {
        Assert.Equal(0, Factory.PaymentGateway.Charges);
        var counts = await CountsAsync();
        Assert.All(counts.Where(row => row.Key.EndsWith("\"OutboxMessages\"", StringComparison.Ordinal)
            || row.Key.EndsWith("\"InboxMessages\"", StringComparison.Ordinal)), row => Assert.Equal(0, row.Value));
    }
}
