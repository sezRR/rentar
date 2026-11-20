using Rentar.Modules.Findeks.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Findeks;

public class FindeksReportTests
{
    [Fact]
    public void MaxRating_IsWhatTheFactorMaximumsAddUpTo()
    {
        Assert.Equal(1900, FindeksReport.MaxRating);
    }

    [Fact]
    public void Rating_AddsUpTheFactors()
    {
        var report = new FindeksReport(100, 200, 30, 40, 5);

        Assert.Equal(375, report.Rating);
    }

    [Fact]
    public void Rating_IsZero_WhenEveryFactorIs()
    {
        Assert.Equal(0, new FindeksReport(0, 0, 0, 0, 0).Rating);
    }

    [Fact]
    public void Rating_IsTheMaximum_WhenEveryFactorIsAtItsMaximum()
    {
        var report = new FindeksReport(
            FindeksReport.MaxCreditProductRepayments,
            FindeksReport.MaxCurrentAccountsAndDebts,
            FindeksReport.MaxLoanUsageIntensity,
            FindeksReport.MaxNewLoansTaken,
            FindeksReport.MaxOtherFactors);

        Assert.Equal(FindeksReport.MaxRating, report.Rating);
    }

    public static TheoryData<int, int, int, int, int> FactorsOutOfRange()
    {
        return new TheoryData<int, int, int, int, int>
        {
            { -1, 0, 0, 0, 0 },
            { 0, -1, 0, 0, 0 },
            { 0, 0, -1, 0, 0 },
            { 0, 0, 0, -1, 0 },
            { 0, 0, 0, 0, -1 },
            { FindeksReport.MaxCreditProductRepayments + 1, 0, 0, 0, 0 },
            { 0, FindeksReport.MaxCurrentAccountsAndDebts + 1, 0, 0, 0 },
            { 0, 0, FindeksReport.MaxLoanUsageIntensity + 1, 0, 0 },
            { 0, 0, 0, FindeksReport.MaxNewLoansTaken + 1, 0 },
            { 0, 0, 0, 0, FindeksReport.MaxOtherFactors + 1 }
        };
    }

    [Theory]
    [MemberData(nameof(FactorsOutOfRange))]
    public void Constructor_RefusesFactorOutsideZeroToItsMaximum(
        int creditProductRepayments,
        int currentAccountsAndDebts,
        int loanUsageIntensity,
        int newLoansTaken,
        int otherFactors)
    {
        Refused.With(DomainMessages.FindeksFactorOutOfRange, () => new FindeksReport(
            creditProductRepayments, currentAccountsAndDebts, loanUsageIntensity, newLoansTaken, otherFactors));
    }
}
