using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application.Queries;

/// <summary>A Findeks credit rating, for no one in particular and not stored. The rating is simulated.</summary>
public sealed record CalculateFindeksRatingQuery;

public sealed class CalculateFindeksRatingQueryHandler : IQueryHandler<CalculateFindeksRatingQuery, int>
{
    /// <remarks>Simulated in memory, so it completes at once; a real bureau would be awaited here.</remarks>
    public Task<int> HandleAsync(CalculateFindeksRatingQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FindeksRating.Calculate());
    }
}
