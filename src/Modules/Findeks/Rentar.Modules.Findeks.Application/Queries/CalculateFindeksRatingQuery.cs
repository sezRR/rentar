using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Application.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application.Queries;

/// <summary>A Findeks credit rating. No credit bureau is connected, so the rating is simulated.</summary>
public sealed record CalculateFindeksRatingQuery;

public sealed class CalculateFindeksRatingQueryHandler : IQueryHandler<CalculateFindeksRatingQuery, int>
{
    /// <remarks>Simulated in memory, so it completes at once; a real bureau would be awaited here.</remarks>
    public Task<int> HandleAsync(CalculateFindeksRatingQuery query, CancellationToken cancellationToken = default)
    {
        // Each factor is simulated up to its maximum.
        var report = new FindeksReport
        {
            CreditProductRepayments = Random.Shared.Next(666), // 665
            CurrentAccountsAndDebts = Random.Shared.Next(666), // 665
            LoanUsageIntensity = Random.Shared.Next(191),      // 190
            NewLoansTaken = Random.Shared.Next(210),           // 209
            OtherFactors = Random.Shared.Next(172),            // 171
        };

        var findeksRating = report.CreditProductRepayments
            + report.CurrentAccountsAndDebts
            + report.LoanUsageIntensity
            + report.NewLoansTaken
            + report.OtherFactors;

        return Task.FromResult(findeksRating);
    }
}
