using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Companies.Domain;

namespace Rentar.Modules.Companies.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Roles are picked by name in a company, so two of its roles never share one.
        builder.HasIndex(role => new { role.CompanyId, role.Name }).IsUnique();
    }
}
