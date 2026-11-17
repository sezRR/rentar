using Rentar.Modules.Findeks.Domain;
using System;

namespace Rentar.Modules.Findeks.Application;

/// <summary>A Findeks credit rating. No credit bureau is connected, so the rating is simulated.</summary>
internal static class FindeksRating
{
    /// <summary>
    /// A rating between 0 and <see cref="FindeksReport.MaxRating"/>: what a report adds up to whose factors are each
    /// simulated up to their maximum.
    /// </summary>
    public static int Calculate()
    {
        var report = new FindeksReport(
            Simulated(FindeksReport.MaxCreditProductRepayments),
            Simulated(FindeksReport.MaxCurrentAccountsAndDebts),
            Simulated(FindeksReport.MaxLoanUsageIntensity),
            Simulated(FindeksReport.MaxNewLoansTaken),
            Simulated(FindeksReport.MaxOtherFactors));

        return report.Rating;
    }

    /// <summary>Between 0 and <paramref name="max"/>, both included.</summary>
    private static int Simulated(int max)
    {
        return Random.Shared.Next(max + 1);
    }
}
