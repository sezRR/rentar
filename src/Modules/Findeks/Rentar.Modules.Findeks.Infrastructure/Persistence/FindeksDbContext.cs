using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Findeks.Infrastructure.Persistence;

public class FindeksDbContext(DbContextOptions<FindeksDbContext> options) : DbContext(options)
{
    public const string Schema = "findeks";

    public DbSet<CustomerScore> CustomerScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FindeksDbContext).Assembly);
        modelBuilder.MapMessaging();
        modelBuilder.UseTimestampWithoutTimeZone();
    }
}
