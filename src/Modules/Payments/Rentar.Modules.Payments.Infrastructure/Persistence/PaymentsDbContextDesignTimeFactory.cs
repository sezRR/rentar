using Microsoft.EntityFrameworkCore.Design;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Payments.Infrastructure.Persistence;

internal sealed class PaymentsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        return new PaymentsDbContext(ModuleDbContextOptions.ForDesignTime<PaymentsDbContext>(PaymentsDbContext.Schema));
    }
}
