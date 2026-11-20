using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Answers the user, or <c>null</c> when there is none with that id.</summary>
public sealed record GetUserByIdQuery(int Id);

public sealed class GetUserByIdQueryHandler(IUserDal userDal) : IQueryHandler<GetUserByIdQuery, User>
{
    public Task<User> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        return userDal.GetAsync(u => u.Id == query.Id, cancellationToken);
    }
}
