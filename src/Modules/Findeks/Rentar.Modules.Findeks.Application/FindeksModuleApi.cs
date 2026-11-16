using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Modules.Findeks.Contracts;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Findeks.Application;

public class FindeksModuleApi(IQueryHandler<CalculateFindeksRatingQuery, int> calculateFindeksRating) : IFindeksModuleApi
{
    public int CalculateFindeksRating()
    {
        return calculateFindeksRating.Handle(new CalculateFindeksRatingQuery());
    }
}
