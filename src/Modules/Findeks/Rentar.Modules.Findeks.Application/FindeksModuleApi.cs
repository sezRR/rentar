using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Modules.Findeks.Contracts;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application;

public class FindeksModuleApi(IQueryHandler<CalculateFindeksRatingQuery, int> calculateFindeksRating) : IFindeksModuleApi
{
    public Task<int> CalculateFindeksRatingAsync(CancellationToken cancellationToken = default)
    {
        return calculateFindeksRating.HandleAsync(new CalculateFindeksRatingQuery(), cancellationToken);
    }
}
