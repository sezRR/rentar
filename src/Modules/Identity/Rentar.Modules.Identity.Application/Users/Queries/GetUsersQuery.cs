using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Every user.</summary>
public sealed record GetUsersQuery;

public sealed class GetUsersQueryHandler(IUserDal userDal) : IQueryHandler<GetUsersQuery, List<User>>
{
    public async Task<List<User>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        return await userDal.GetAllAsync(cancellationToken);
    }
}
