using Rentar.Modules.Findeks.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Findeks;

public class CustomerScoreTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(950)]
    [InlineData(FindeksReport.MaxRating)]
    public void Constructor_TakesRatingFromZeroToTheMaximum(int findeksRating)
    {
        var score = new CustomerScore(customerId: 4, findeksRating);

        Assert.Equal(4, score.CustomerId);
        Assert.Equal(findeksRating, score.FindeksRating);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(FindeksReport.MaxRating + 1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Constructor_RefusesRatingOutsideZeroToTheMaximum(int findeksRating)
    {
        Refused.With(DomainMessages.FindeksRatingOutOfRange, () => new CustomerScore(customerId: 4, findeksRating));
    }
}
