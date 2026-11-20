using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Companies.Domain;

namespace Rentar.Modules.Companies.Infrastructure.Persistence.Configurations;

/// <summary>
/// A company owns its employees and its roles (one-to-many each); they mean nothing without it.
/// </summary>
internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasMany(company => company.Employees)
            .WithOne(employee => employee.Company)
            .HasForeignKey(employee => employee.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(company => company.Roles)
            .WithOne(role => role.Company)
            .HasForeignKey(role => role.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
