using Microsoft.EntityFrameworkCore;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Payments.Infrastructure.Persistence;

/// <summary>
/// Payments keeps no data of its own (no card details are stored); its schema holds only the outbox of the payment
/// outcomes it publishes and the inbox of the rentals it took payments for.
/// </summary>
public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public const string Schema = "payments";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.MapMessaging();
        modelBuilder.UseTimestampWithoutTimeZone();
    }
}
