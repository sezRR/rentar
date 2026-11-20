using Microsoft.EntityFrameworkCore.Design;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

internal sealed class CompaniesDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CompaniesDbContext>
{
    public CompaniesDbContext CreateDbContext(string[] args)
    {
        return new CompaniesDbContext(ModuleDbContextOptions.ForDesignTime<CompaniesDbContext>(CompaniesDbContext.Schema));
    }
}
