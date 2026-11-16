using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>The users whose Findeks rating is at least <see cref="MinFindeksRating"/>.</summary>
public sealed record GetUsersByMinFindeksRatingQuery(int MinFindeksRating);

public sealed class GetUsersByMinFindeksRatingQueryHandler(IUserDal userDal) : IQueryHandler<GetUsersByMinFindeksRatingQuery, List<User>>
{
    public Task<List<User>> HandleAsync(GetUsersByMinFindeksRatingQuery query, CancellationToken cancellationToken = default)
    {
        return userDal.GetAllAsync(u => u.FindeksRating >= query.MinFindeksRating, cancellationToken);
    }
}
