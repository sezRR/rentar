using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Identity.Domain;

namespace Rentar.Modules.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Users and operation claims are many-to-many, joined through <see cref="UserOperationClaim"/>.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Users are looked up by email, which identifies them.
        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasMany(user => user.OperationClaims)
            .WithMany(claim => claim.Users)
            .UsingEntity<UserOperationClaim>(
                toClaim => toClaim.HasOne<OperationClaim>().WithMany().HasForeignKey(grant => grant.OperationClaimId),
                toUser => toUser.HasOne<User>().WithMany().HasForeignKey(grant => grant.UserId),
                grant => grant.HasKey(g => new { g.UserId, g.OperationClaimId }));
    }
}
