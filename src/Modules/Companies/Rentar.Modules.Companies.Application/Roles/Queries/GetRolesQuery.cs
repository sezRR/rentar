using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Roles.Queries;

/// <summary>
/// The company's roles with their permissions, or one page of them, for any of its employees; throws
/// <see cref="NotFoundException"/> when <see cref="ActorUserId"/> does not work there.
/// </summary>
public sealed record GetRolesQuery(int ActorUserId, int CompanyId, PageRequest Page = null) : IPagedQuery;

public sealed class GetRolesQueryHandler(IEmployeeDal employeeDal, IRoleDal roleDal) : IQueryHandler<GetRolesQuery, Page<Role>>
{
    public async Task<Page<Role>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        await employeeDal.RequireEmployeeAsync(query.CompanyId, query.ActorUserId, cancellationToken);

        return await roleDal.GetPageAsync(r => r.CompanyId == query.CompanyId, query.Page, cancellationToken);
    }
}
