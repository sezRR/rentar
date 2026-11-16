using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Identity.Domain;

namespace Rentar.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class OperationClaimConfiguration : IEntityTypeConfiguration<OperationClaim>
{
    public void Configure(EntityTypeBuilder<OperationClaim> builder)
    {
        // A claim's name is the role an access token carries, so two claims never share one.
        builder.HasIndex(claim => claim.Name).IsUnique();
    }
}
