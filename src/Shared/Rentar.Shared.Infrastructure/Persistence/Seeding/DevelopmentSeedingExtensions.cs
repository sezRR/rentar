using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Rentar.Shared.Infrastructure.Persistence.Seeding;

public static class DevelopmentSeedingExtensions
{
    /// <summary>
    /// Seeds only Development hosts that opted in. Run after migrations and before serving requests.
    /// The whole run commits together, and a database lock serializes hosts that start at the same time.
    /// </summary>
    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment()
            || !configuration.GetValue<bool>("Database:SeedOnStartup"))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var seeders = scope.ServiceProvider.GetServices<IDevelopmentDataSeeder>().OrderBy(seeder => seeder.Order).ToList();
        using (var transaction = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TransactionManager.DefaultTimeout },
            TransactionScopeAsyncFlowOption.Enabled))
        {
            // Closing the connection returns it to Npgsql's ambient-transaction pool; the module contexts reuse it.
            // Keeping it open while they connect would require more than one connection in the transaction.
            await using (var connection = new NpgsqlConnection(DatabaseConnection.Resolve(configuration)))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(72636874617201)", connection);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            var context = new DevelopmentSeedContext();
            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(context, cancellationToken);
            }

            transaction.Complete();
        }

        services.GetRequiredService<ILoggerFactory>().CreateLogger("DevelopmentSeeding")
            .LogInformation("Development demo data is ready ({Modules} modules). Existing records were preserved.", seeders.Count);
    }
}
