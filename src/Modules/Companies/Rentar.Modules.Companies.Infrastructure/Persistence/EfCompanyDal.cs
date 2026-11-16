using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

public class EfCompanyDal(IDbContextFactory<CompaniesDbContext> contextFactory)
    : EfEntityRepositoryBase<Company, CompaniesDbContext>(contextFactory), ICompanyDal
{
    public async Task RegisterAsync(Company company, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        // Unlike AddAsync, Add follows the navigations, so the roles, employees and role grants go in the same save.
        context.Companies.Add(company);
        await context.SaveChangesAsync(cancellationToken);
    }
}
