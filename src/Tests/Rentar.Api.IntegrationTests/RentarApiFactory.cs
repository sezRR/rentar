using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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

        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Clock));
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

    /// <summary>Stores the user as inactive (<c>Status</c> false). No v2 endpoint does, and updating a user activates it.</summary>
    public async Task DeactivateUserAsync(int userId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("""UPDATE identity."Users" SET "Status" = false WHERE "Id" = @userId""", connection);
        command.Parameters.AddWithValue("userId", userId);
        await command.ExecuteNonQueryAsync();
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
    /// Empties every module's tables, the in-process cache and the clock so each test starts from a known state.
    /// </summary>
    public async Task ResetAsync()
    {
        Clock.Reset();

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
