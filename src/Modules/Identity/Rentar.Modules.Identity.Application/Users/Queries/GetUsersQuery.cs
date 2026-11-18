using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>Every user, or one page of them.</summary>
public sealed record GetUsersQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetUsersQueryHandler(IUserDal userDal) : IQueryHandler<GetUsersQuery, Page<User>>
{
    public Task<Page<User>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        return userDal.GetPageAsync(query.Page, cancellationToken);
    }
}
