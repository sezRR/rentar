using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

public class CompaniesDbContext(DbContextOptions<CompaniesDbContext> options) : DbContext(options)
{
    public const string Schema = "companies";

    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompaniesDbContext).Assembly);
        modelBuilder.MapMessaging();
        modelBuilder.UseTimestampWithoutTimeZone();
    }
}
