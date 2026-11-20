#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Findeks.Application.Commands;
using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Presentation.Endpoints.V2;

/// <summary>
/// A customer's scores belong to the customer, so they are addressed under it: <c>/customers/{customerId}/score</c>.
/// </summary>
public class CustomerScoresEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Findeks", "customers/{customerId:int}/score", 2.0);

        group.MapGet("/", Get)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Calculate)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<CustomerScore>, ProblemHttpResult>> Get(
        IQueryHandler<GetCustomerScoreQuery, CustomerScore> getCustomerScore,
        int customerId,
        CancellationToken cancellationToken)
    {
        var score = await getCustomerScore.HandleAsync(new GetCustomerScoreQuery(customerId), cancellationToken);
        return score is null ? RestResults.NotFound("Score of customer", customerId) : TypedResults.Ok(score);
    }

    /// <summary>
    /// Calculates the customer's Findeks rating and stores it, replacing an earlier score. Each call calculates anew,
    /// so it is a POST rather than a PUT.
    /// </summary>
    private static async Task<Ok<CustomerScore>> Calculate(
        ICommandHandler<CalculateCustomerScoreCommand, CustomerScore> calculateCustomerScore,
        int customerId,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await calculateCustomerScore.HandleAsync(new CalculateCustomerScoreCommand(customerId), cancellationToken));
    }
}
