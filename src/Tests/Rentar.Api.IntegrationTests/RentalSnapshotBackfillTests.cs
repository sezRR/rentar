using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rentar.Api.IntegrationTests.V2;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Rentals.Domain;
using Rentar.Modules.Rentals.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// <c>AddRentalSnapshots</c> fills in the snapshot of every rental stored before it, from what the other modules know
/// when it runs. The rentals schema is stepped back to the migration before it, so the rows are stored the old way.
/// </summary>
public class RentalSnapshotBackfillTests(RentarApiFactory factory) : V2TestBase(factory)
{
    private const string MigrationBeforeSnapshots = "20261117224545_PreventOverlappingRentals";

    [Fact]
    public async Task AddRentalSnapshots_CopiesCarCustomerAndPrice_IntoRentalsStoredBefore()
    {
        var owner = await SignInAsNewUserAsync("renter@example.com", "Ada");
        var company = await RegisterCompanyAsync();
        var customer = await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(owner.Id, company.Id));
        var privateCustomer = await CreateCustomerAsync("private@example.com");
        var car = await CreateCarAsync();
        var rentDate = new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);

        await using (var database = await NewRentalsDatabaseAsync())
        {
            var migrator = database.GetService<IMigrator>();
            await migrator.MigrateAsync(MigrationBeforeSnapshots, TestContext.Current.CancellationToken);
            try
            {
                // Two days and an hour: the started third day is billed too.
                await StoreOldRentalAsync(car.Id, customer.Id, rentDate, rentDate.AddDays(2).AddHours(1));
                // A rental of a car Fleet no longer knows, by a customer with no company.
                await StoreOldRentalAsync(car.Id + 1, privateCustomer.Id, rentDate.AddDays(10), rentDate.AddDays(11));
            }
            finally
            {
                await migrator.MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            }
        }

        var rentals = (await (await Client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>())
            .OrderBy(rental => rental.Id)
            .ToList();

        Assert.Equal(2, rentals.Count);
        var booked = rentals[0];
        Assert.Equal(new RentalCarSnapshot(car.BrandId, "Toyota", car.ColorId, "Red", 2021, "Corolla 1.6"), booked.Car);
        Assert.Equal(new RentalCustomerSnapshot("Ada", "Tetik", company.Id, "Rentar Logistics"), booked.Customer);
        Assert.Equal(950m, booked.DailyPrice);
        Assert.Equal(950m * 3, booked.TotalPrice);

        var ofMissingCar = rentals[1];
        Assert.Equal(new RentalCarSnapshot(0, "", 0, "", 0, ""), ofMissingCar.Car);
        Assert.Equal(new RentalCustomerSnapshot("Sezer", "Tetik", null, null), ofMissingCar.Customer);
        Assert.Equal(0m, ofMissingCar.DailyPrice);
        Assert.Equal(0m, ofMissingCar.TotalPrice);
    }

    private async Task<RentalsDbContext> NewRentalsDatabaseAsync()
    {
        var contexts = Factory.Services.GetRequiredService<IDbContextFactory<RentalsDbContext>>();
        return await contexts.CreateDbContextAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Stores a rental with only the columns rentals had before snapshots.</summary>
    private async Task StoreOldRentalAsync(int carId, int customerId, DateTime rentDate, DateTime returnDate)
    {
        await using var connection = new NpgsqlConnection(Factory.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO rentals."Rentals" ("CarId", "CustomerId", "RentDate", "ReturnDate", "Status")
            VALUES (@carId, @customerId, @rentDate, @returnDate, 'Confirmed')
            """,
            connection);
        command.Parameters.AddWithValue("carId", carId);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("rentDate", rentDate);
        command.Parameters.AddWithValue("returnDate", returnDate);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
