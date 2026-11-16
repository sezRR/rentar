using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Roles.Queries;

/// <summary>
/// The company's roles with their permissions, for any of its employees; throws <see cref="NotFoundException"/> when
/// <see cref="ActorUserId"/> does not work there.
/// </summary>
public sealed record GetRolesQuery(int ActorUserId, int CompanyId);

public sealed class GetRolesQueryHandler(IEmployeeDal employeeDal, IRoleDal roleDal) : IQueryHandler<GetRolesQuery, List<Role>>
{
    public async Task<List<Role>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        await employeeDal.RequireEmployeeAsync(query.CompanyId, query.ActorUserId, cancellationToken);

        var roles = await roleDal.GetAllAsync(r => r.CompanyId == query.CompanyId, cancellationToken);
        return roles.OrderBy(r => r.Id).ToList();
    }
}
