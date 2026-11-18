using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Infrastructure.Persistence;

internal sealed class FindeksDevelopmentDataSeeder(IDbContextFactory<FindeksDbContext> contextFactory) : IDevelopmentDataSeeder
{
    public int Order => 50;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (key, rating) in new[] { ("customers.member", 1500), ("customers.renter", 1350) })
        {
            var customerId = context.GetId(key);
            if (!await database.CustomerScores.AnyAsync(score => score.CustomerId == customerId, cancellationToken))
            {
                database.CustomerScores.Add(new CustomerScore(customerId, rating));
            }
        }

        await database.SaveChangesAsync(cancellationToken);
    }
}
