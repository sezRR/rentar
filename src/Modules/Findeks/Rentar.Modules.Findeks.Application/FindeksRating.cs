using Rentar.Modules.Findeks.Domain;
using System;

namespace Rentar.Modules.Findeks.Application;

/// <summary>A Findeks credit rating. No credit bureau is connected, so the rating is simulated.</summary>
internal static class FindeksRating
{
    /// <summary>A rating between 0 and 1900: the sum of the report's factors, each simulated up to its maximum.</summary>
    public static int Calculate()
    {
        var report = new FindeksReport
        {
            CreditProductRepayments = Random.Shared.Next(666), // 665
            CurrentAccountsAndDebts = Random.Shared.Next(666), // 665
            LoanUsageIntensity = Random.Shared.Next(191),      // 190
            NewLoansTaken = Random.Shared.Next(210),           // 209
            OtherFactors = Random.Shared.Next(172),            // 171
        };

        return report.CreditProductRepayments
            + report.CurrentAccountsAndDebts
            + report.LoanUsageIntensity
            + report.NewLoansTaken
            + report.OtherFactors;
    }
}
