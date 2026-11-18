using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Infrastructure.Persistence;

internal sealed class CustomersDevelopmentDataSeeder(IDbContextFactory<CustomersDbContext> contextFactory) : IDevelopmentDataSeeder
{
    public int Order => 40;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (key, userId, companyId) in new[]
        {
            ("customers.member", context.GetId("identity.member"), (int?)context.GetId("companies.demo")),
            ("customers.renter", context.GetId("identity.renter"), (int?)null)
        })
        {
            var customer = await database.Customers.SingleOrDefaultAsync(customer => customer.UserId == userId, cancellationToken);
            if (customer is null)
            {
                customer = new Customer(userId, companyId);
                database.Customers.Add(customer);
                await database.SaveChangesAsync(cancellationToken);
            }

            context.SetId(key, customer.Id);
        }
    }
}
