using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Findeks.Application;
using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Presentation.Endpoints.V1;

public class FindeksEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Findeks", "findeks", 1.0);

        group.MapGet("/calculate", Calculate);
    }

    private static async Task<Ok<V1Response<int>>> Calculate(
        IQueryHandler<CalculateFindeksRatingQuery, int> calculateFindeksRating,
        CancellationToken cancellationToken)
    {
        var rating = await calculateFindeksRating.HandleAsync(new CalculateFindeksRatingQuery(), cancellationToken);
        return V1Results.Ok(rating, Messages.FindeksRatingCalculated);
    }
}
