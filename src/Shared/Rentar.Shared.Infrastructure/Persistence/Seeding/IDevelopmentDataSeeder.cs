using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Persistence.Seeding;

/// <summary>A module seeds only its own data. Lower orders run first, so dependencies can export their ids.</summary>
public interface IDevelopmentDataSeeder
{
    int Order { get; }

    Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default);
}
