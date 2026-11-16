using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Answers the user, or <c>null</c> when no user has that email.</summary>
public sealed record GetUserByEmailQuery(string Email);

public sealed class GetUserByEmailQueryHandler(IUserDal userDal) : IQueryHandler<GetUserByEmailQuery, User>
{
    public User Handle(GetUserByEmailQuery query)
    {
        return userDal.Get(u => u.Email == query.Email);
    }
}
