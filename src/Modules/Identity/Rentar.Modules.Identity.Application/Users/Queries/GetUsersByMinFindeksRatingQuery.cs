using System.Collections.Generic;
using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Identity.Application.Users.Queries;

/// <summary>The users whose Findeks rating is at least <see cref="MinFindeksRating"/>.</summary>
public sealed record GetUsersByMinFindeksRatingQuery(int MinFindeksRating);

public sealed class GetUsersByMinFindeksRatingQueryHandler(IUserDal userDal) : IQueryHandler<GetUsersByMinFindeksRatingQuery, List<User>>
{
    public List<User> Handle(GetUsersByMinFindeksRatingQuery query)
    {
        return userDal.GetAll(u => u.FindeksRating >= query.MinFindeksRating);
    }
}
