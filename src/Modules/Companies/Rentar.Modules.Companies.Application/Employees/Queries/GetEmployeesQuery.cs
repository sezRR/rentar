using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using Rentar.Shared.Kernel.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Employees.Queries;

/// <summary>
/// The company's employees with their names and roles, or one page of them, for an employee
/// <see cref="ActorUserId"/> holding <see cref="CompanyPermissions.EmployeesRead"/>; throws
/// <see cref="NotFoundException"/> or <see cref="ForbiddenException"/> otherwise.
/// </summary>
public sealed record GetEmployeesQuery(int ActorUserId, int CompanyId, PageRequest Page = null) : IPagedQuery;

public sealed class GetEmployeesQueryHandler(IEmployeeDal employeeDal, IIdentityModuleApi identityModule)
    : IQueryHandler<GetEmployeesQuery, Page<EmployeeResponse>>
{
    public async Task<Page<EmployeeResponse>> HandleAsync(GetEmployeesQuery query, CancellationToken cancellationToken = default)
    {
        await employeeDal.RequirePermissionAsync(query.CompanyId, query.ActorUserId, CompanyPermissions.EmployeesRead, cancellationToken);

        var employees = await employeeDal.GetPageByCompanyAsync(query.CompanyId, query.Page, cancellationToken);

        // An employee only holds the id of its Identity user, so the names come through that module's public API.
        var users = (await identityModule.GetUsersAsync(employees.Items.Select(e => e.UserId).Distinct().ToList(), cancellationToken))
            .ToDictionary(u => u.UserId);

        return employees.Map(employee =>
        {
            var user = users.GetValueOrDefault(employee.UserId);
            return new EmployeeResponse(
                employee.Id,
                employee.CompanyId,
                employee.UserId,
                user?.FirstName,
                user?.LastName,
                employee.Roles.OrderBy(r => r.Id).ToList());
        });
    }
}
