using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>
/// The users with the email as a list, so a filtered list pages like any other; emails are unique, so it holds at
/// most one. <see cref="GetUserByEmailQuery"/> answers the user itself.
/// </summary>
public sealed record GetUsersByEmailQuery(string Email, PageRequest Page = null) : IPagedQuery;

public sealed class GetUsersByEmailQueryHandler(IUserDal userDal) : IQueryHandler<GetUsersByEmailQuery, Page<User>>
{
    public Task<Page<User>> HandleAsync(GetUsersByEmailQuery query, CancellationToken cancellationToken = default)
    {
        return userDal.GetPageAsync(u => u.Email == query.Email, query.Page, cancellationToken);
    }
}
