using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Findeks.Domain;

/// <summary>
/// The factors a Findeks credit rating is made of. Their maximums add up to 1900, the highest rating.
/// </summary>
public class FindeksReport : IEntity
{
    public int CreditProductRepayments { get; init; }
    public int CurrentAccountsAndDebts { get; init; }
    public int LoanUsageIntensity { get; init; }
    public int NewLoansTaken { get; init; }
    public int OtherFactors { get; init; }
    public int MinimumFindeksRating { get; init; }
}
