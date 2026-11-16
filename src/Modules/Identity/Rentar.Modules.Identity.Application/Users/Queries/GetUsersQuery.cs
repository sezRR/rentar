using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Every user; throws <see cref="UnavailableException"/> during the 22:00 maintenance hour.</summary>
public sealed record GetUsersQuery;

public sealed class GetUsersQueryHandler(IUserDal userDal, TimeProvider timeProvider) : IQueryHandler<GetUsersQuery, List<User>>
{
    public async Task<List<User>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        if (timeProvider.GetLocalNow().Hour == 22)
        {
            throw new UnavailableException(Messages.MaintenanceTime);
        }

        return await userDal.GetAllAsync(cancellationToken);
    }
}
