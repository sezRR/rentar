using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Companies.Domain;

namespace Rentar.Modules.Companies.Infrastructure.Persistence.Configurations;

/// <summary>
/// Employees and roles are many-to-many, joined through <see cref="EmployeeRole"/>.
/// </summary>
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // A user works at a company at most once. The index also serves listing a company's employees.
        builder.HasIndex(employee => new { employee.CompanyId, employee.UserId }).IsUnique();

        // The user lives in the Identity schema, so there is no foreign key to index it; companies are listed by user.
        builder.HasIndex(employee => employee.UserId);

        builder.HasMany(employee => employee.Roles)
            .WithMany(role => role.Employees)
            .UsingEntity<EmployeeRole>(
                toRole => toRole.HasOne<Role>().WithMany().HasForeignKey(grant => grant.RoleId),
                toEmployee => toEmployee.HasOne<Employee>().WithMany().HasForeignKey(grant => grant.EmployeeId),
                grant => grant.HasKey(g => new { g.EmployeeId, g.RoleId }));
    }
}
