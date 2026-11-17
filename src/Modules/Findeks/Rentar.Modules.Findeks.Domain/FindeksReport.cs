using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Findeks.Domain;

/// <summary>
/// The factors a Findeks credit rating is made of, each between 0 and its maximum; the constructor refuses others with
/// <see cref="BusinessRuleException"/>. Their maximums add up to <see cref="MaxRating"/>, the highest rating.
/// </summary>
public class FindeksReport : IEntity
{
    public const int MaxCreditProductRepayments = 665;
    public const int MaxCurrentAccountsAndDebts = 665;
    public const int MaxLoanUsageIntensity = 190;
    public const int MaxNewLoansTaken = 209;
    public const int MaxOtherFactors = 171;

    public const int MaxRating = MaxCreditProductRepayments
        + MaxCurrentAccountsAndDebts
        + MaxLoanUsageIntensity
        + MaxNewLoansTaken
        + MaxOtherFactors;

    public FindeksReport(
        int creditProductRepayments,
        int currentAccountsAndDebts,
        int loanUsageIntensity,
        int newLoansTaken,
        int otherFactors)
    {
        CreditProductRepayments = Factor(creditProductRepayments, MaxCreditProductRepayments);
        CurrentAccountsAndDebts = Factor(currentAccountsAndDebts, MaxCurrentAccountsAndDebts);
        LoanUsageIntensity = Factor(loanUsageIntensity, MaxLoanUsageIntensity);
        NewLoansTaken = Factor(newLoansTaken, MaxNewLoansTaken);
        OtherFactors = Factor(otherFactors, MaxOtherFactors);
    }

    public int CreditProductRepayments { get; private init; }
    public int CurrentAccountsAndDebts { get; private init; }
    public int LoanUsageIntensity { get; private init; }
    public int NewLoansTaken { get; private init; }
    public int OtherFactors { get; private init; }

    /// <summary>The rating the report adds up to, between 0 and <see cref="MaxRating"/>.</summary>
    public int Rating => CreditProductRepayments + CurrentAccountsAndDebts + LoanUsageIntensity + NewLoansTaken + OtherFactors;

    private static int Factor(int value, int max)
    {
        return value < 0 || value > max ? throw new BusinessRuleException(DomainMessages.FindeksFactorOutOfRange) : value;
    }
}
