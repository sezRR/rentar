using Microsoft.EntityFrameworkCore.Design;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Findeks.Infrastructure.Persistence;

internal sealed class FindeksDbContextDesignTimeFactory : IDesignTimeDbContextFactory<FindeksDbContext>
{
    public FindeksDbContext CreateDbContext(string[] args)
    {
        return new FindeksDbContext(ModuleDbContextOptions.ForDesignTime<FindeksDbContext>(FindeksDbContext.Schema));
    }
}
