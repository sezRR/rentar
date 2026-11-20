using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rentar.Modules.Payments.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// Boots the API in memory against a throwaway PostgreSQL container, with every module's schema migrated.
/// </summary>
public class RentarApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("rentar_test")
        .WithUsername("rentar")
        .WithPassword("rentar")
        .Build();

    private IReadOnlyList<string> _tables = [];

    public TestClock Clock { get; } = new();

    public TestPaymentGateway PaymentGateway { get; } = new();

    public string ConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Touching Services builds the host, which picks up the container's connection string below.
        await Services.MigrateModuleDatabasesAsync();

        _tables = Services.GetServices<IModuleDatabase>()
            .SelectMany(database => database.GetTableNames())
            .ToList();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"ConnectionStrings:{DatabaseConnection.ConnectionStringName}", ConnectionString);
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Database:SeedOnStartup", "false");
        // Maintenance tests explicitly enable their schedule; other tests must not depend on the wall clock.
        builder.UseSetting("Maintenance:Enabled", "false");
        // A test delivers the events itself with DeliverEventsAsync, so it knows the reactions have run when it asserts.
        builder.UseSetting($"{OutboxOptions.SectionName}:{nameof(OutboxOptions.DeliverInBackground)}", "false");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IPaymentGateway>(PaymentGateway);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Makes the database refuse every <paramref name="operation"/> (<c>INSERT</c>, <c>UPDATE</c> or <c>DELETE</c>) on a
    /// table until the result is disposed, to see what a use case leaves behind when a write fails halfway.
    /// </summary>
    public async Task<IAsyncDisposable> RefuseAsync(string schema, string table, string operation)
    {
        var function = $"\"{schema}\".refuse_for_test";
        var trigger = $"refuse_{operation.ToLowerInvariant()}_for_test";
        var target = $"\"{schema}\".\"{table}\"";

        await ExecuteAsync(
            $"CREATE FUNCTION {function}() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'refused by test'; END $$;" +
            $"CREATE TRIGGER {trigger} BEFORE {operation} ON {target} FOR EACH ROW EXECUTE FUNCTION {function}();");

        return new AsyncDisposable(() => ExecuteAsync($"DROP TRIGGER {trigger} ON {target}; DROP FUNCTION {function}();"));
    }

    /// <summary>
    /// Grants the user the named operation claims, creating the claims that do not exist yet. No endpoint grants claims.
    /// </summary>
    public async Task GrantAsync(int userId, params string[] claims)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        foreach (var claim in claims)
        {
            await using var command = new NpgsqlCommand(
                """
                INSERT INTO identity."OperationClaims" ("Name") VALUES (@claim) ON CONFLICT ("Name") DO NOTHING;
                INSERT INTO identity."UserOperationClaims" ("UserId", "OperationClaimId")
                SELECT @userId, "Id" FROM identity."OperationClaims" WHERE "Name" = @claim;
                """,
                connection);
            command.Parameters.AddWithValue("claim", claim);
            command.Parameters.AddWithValue("userId", userId);
            await command.ExecuteNonQueryAsync();
        }
    }

    /// <summary>The car every rental stored by <see cref="InsertRental"/> was booked as.</summary>
    public static readonly RentalCarSnapshot StoredCar = new(
        BrandId: 404, BrandName: "Lada", ColorId: 404, ColorName: "Beige", ModelYear: 1990, Description: "Niva 1.7");

    /// <summary>The customer every rental stored by <see cref="InsertRental"/> was booked as.</summary>
    public static readonly RentalCustomerSnapshot StoredCustomer = new(
        FirstName: "Gone", LastName: "Customer", CompanyId: null, CompanyName: null);

    /// <summary>The daily price every rental stored by <see cref="InsertRental"/> was booked at.</summary>
    public const decimal StoredDailyPrice = 400m;

    /// <summary>
    /// Stores a rental of a car Fleet does not know, as deleting a rented car left behind before it asked Rentals.
    /// No endpoint does: creating a rental asks Fleet for its car, and a rented car can not be deleted. The rental
    /// was booked as <see cref="StoredCar"/> and <see cref="StoredCustomer"/>.
    /// </summary>
    public async Task StoreRentalOfMissingCarAsync(int carId, int customerId, DateTime rentDate, DateTime returnDate)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = InsertRental(connection, carId, customerId, rentDate, returnDate, "Confirmed");
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// A command storing a rental row directly, booked as <see cref="StoredCar"/> and <see cref="StoredCustomer"/> at
    /// <see cref="StoredDailyPrice"/> a day, for tests that need what no endpoint stores.
    /// </summary>
    public static NpgsqlCommand InsertRental(
        NpgsqlConnection connection, int carId, int customerId, DateTime rentDate, DateTime returnDate, string status)
    {
        var command = new NpgsqlCommand(
            """
            INSERT INTO rentals."Rentals" (
                "CarId", "CustomerId", "RentDate", "ReturnDate", "Status",
                "Car_BrandId", "Car_BrandName", "Car_ColorId", "Car_ColorName", "Car_ModelYear", "Car_Description",
                "Customer_FirstName", "Customer_LastName", "Customer_CompanyId", "Customer_CompanyName",
                "DailyPrice", "TotalPrice")
            VALUES (
                @carId, @customerId, @rentDate, @returnDate, @status,
                @brandId, @brandName, @colorId, @colorName, @modelYear, @description,
                @firstName, @lastName, NULL, NULL,
                @dailyPrice, @dailyPrice)
            """,
            connection);
        command.Parameters.AddWithValue("carId", carId);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("rentDate", rentDate);
        command.Parameters.AddWithValue("returnDate", returnDate);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("brandId", StoredCar.BrandId);
        command.Parameters.AddWithValue("brandName", StoredCar.BrandName);
        command.Parameters.AddWithValue("colorId", StoredCar.ColorId);
        command.Parameters.AddWithValue("colorName", StoredCar.ColorName);
        command.Parameters.AddWithValue("modelYear", StoredCar.ModelYear);
        command.Parameters.AddWithValue("description", StoredCar.Description);
        command.Parameters.AddWithValue("firstName", StoredCustomer.FirstName);
        command.Parameters.AddWithValue("lastName", StoredCustomer.LastName);
        command.Parameters.AddWithValue("dailyPrice", StoredDailyPrice);
        return command;
    }

    /// <summary>
    /// Delivers every module's due outbox messages, and the events their reactions publish in turn, until a pass
    /// delivers nothing more. A message whose delivery failed is tried again only once it is due again.
    /// </summary>
    public async Task DeliverEventsAsync()
    {
        var processor = Services.GetRequiredService<OutboxProcessor>();

        for (var pass = 0; pass < 20 && await processor.ProcessAsync(TestContext.Current.CancellationToken) > 0; pass++)
        {
        }
    }

    /// <summary>The messages in a module's outbox, oldest first.</summary>
    public async Task<List<OutboxRow>> OutboxAsync(string schema)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"SELECT \"Type\", \"ProcessedOn\" IS NOT NULL, \"Attempts\", \"Error\" FROM \"{schema}\".\"OutboxMessages\" ORDER BY \"OccurredOn\", \"Id\"",
            connection);

        var rows = new List<OutboxRow>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var type = reader.GetString(0);
            rows.Add(new OutboxRow(
                type[(type.LastIndexOf('.') + 1)..],
                reader.GetBoolean(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return rows;
    }

    /// <summary>
    /// Marks every message of a module's outbox as not delivered, as when delivery stopped after the subscribers handled
    /// a message but before the outbox recorded it, so the next delivery sends them again.
    /// </summary>
    public Task MarkOutboxUndeliveredAsync(string schema)
    {
        return ExecuteAsync($"UPDATE \"{schema}\".\"OutboxMessages\" SET \"ProcessedOn\" = NULL");
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class AsyncDisposable(Func<Task> dispose) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await dispose();
        }
    }

    /// <summary>
    /// Empties every module's tables, the in-process cache, the clock and the payment gateway so each test starts from a
    /// known state.
    /// </summary>
    public async Task ResetAsync()
    {
        Clock.Reset();
        PaymentGateway.Reset();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        var truncate = $"TRUNCATE {string.Join(", ", _tables)} RESTART IDENTITY CASCADE;";
        await using var command = new NpgsqlCommand(truncate, connection);
        await command.ExecuteNonQueryAsync();

        // The caching aspect keeps results in the singleton cache, which outlives a single test.
        if (Services.GetService<IMemoryCache>() is MemoryCache cache)
        {
            cache.Clear();
        }
    }
}
