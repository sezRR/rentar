using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Answers the user, or <c>null</c> when no user has that email.</summary>
public sealed record GetUserByEmailQuery(string Email);

public sealed class GetUserByEmailQueryHandler(IUserDal userDal) : IQueryHandler<GetUserByEmailQuery, User>
{
    public Task<User> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
    {
        return userDal.GetAsync(u => u.Email == query.Email, cancellationToken);
    }
}
