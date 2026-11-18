using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Kernel.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

public class EfEmployeeDal(IDbContextFactory<CompaniesDbContext> contextFactory)
    : EfEntityRepositoryBase<Employee, CompaniesDbContext>(contextFactory), IEmployeeDal
{
    /// <summary>Ids increase as employees join, so paging by id lists them in the order they joined.</summary>
    private static readonly PageKey<Employee> JoinOrder = new(employee => employee.Id);

    public async Task<List<Employee>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return (await GetPageByCompanyAsync(companyId, null, cancellationToken)).Items;
    }

    public async Task<Page<Employee>> GetPageByCompanyAsync(int companyId, PageRequest page, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        // The skip navigation reads through the EmployeeRoles join table.
        return await context.Employees
            .Include(e => e.Roles)
            .Where(e => e.CompanyId == companyId)
            .ToPageAsync(JoinOrder, page, cancellationToken);
    }

    public async Task<Employee> GetByUserAsync(int companyId, int userId, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Employees
            .Include(e => e.Roles)
            .Include(e => e.Company)
            .SingleOrDefaultAsync(e => e.CompanyId == companyId && e.UserId == userId, cancellationToken);
    }

    public async Task AddWithRolesAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        // The roles are stored already: attached as they are, only the employee and its role grants are inserted.
        context.Roles.AttachRange(employee.Roles);
        context.Employees.Add(employee);
        await context.SaveChangesAsync(cancellationToken);
    }
}
