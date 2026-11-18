using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Infrastructure.Persistence;

internal sealed class RentalsDevelopmentDataSeeder(IDbContextFactory<RentalsDbContext> contextFactory, TimeProvider timeProvider)
    : IDevelopmentDataSeeder
{
    public int Order => 60;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var today = timeProvider.GetLocalNow().Date;
        foreach (var (customerKey, carKey, offset) in new[]
        {
            ("customers.member", "fleet.corolla", -3),
            ("customers.renter", "fleet.bmw", 2)
        })
        {
            var customerId = context.GetId(customerKey);
            var carId = context.GetId(carKey);
            // Once the customer has any history, leave it alone: restarts must not add another moving date window.
            if (await database.Rentals.AnyAsync(rental => rental.CustomerId == customerId, cancellationToken))
            {
                continue;
            }

            var start = today.AddDays(offset).AddHours(10);
            var end = start.AddDays(2);
            if (await database.Rentals.AnyAsync(
                rental => rental.CarId == carId && rental.Status != RentalStatus.Cancelled
                    && rental.RentDate <= end && rental.ReturnDate >= start, cancellationToken))
            {
                continue;
            }

            // Demo history, not a real booking: no outbox event or gateway charge is produced.
            database.Rentals.Add(new Rental(carId, customerId, start, end).Confirmed());
            await database.SaveChangesAsync(cancellationToken);
        }
    }
}
