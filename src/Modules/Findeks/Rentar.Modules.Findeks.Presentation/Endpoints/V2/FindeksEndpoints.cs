#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Presentation.Endpoints.V2;

public class FindeksEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Findeks", "findeks", 2.0);

        group.MapGet("/rating", GetRating);
    }

    /// <summary>
    /// Calculates a Findeks rating. Calculating it changes nothing, so it is read with GET.
    /// </summary>
    private static async Task<Ok<FindeksRatingResponse>> GetRating(
        IQueryHandler<CalculateFindeksRatingQuery, int> calculateFindeksRating,
        CancellationToken cancellationToken)
    {
        var rating = await calculateFindeksRating.HandleAsync(new CalculateFindeksRatingQuery(), cancellationToken);
        return TypedResults.Ok(new FindeksRatingResponse(rating));
    }
}
