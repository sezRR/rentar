using Rentar.Modules.Fleet.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Fleet;

public class BrandTests
{
    [Theory]
    [InlineData("VW")]
    [InlineData("Toyota")]
    public void Constructor_TakesNameOfAtLeastTheMinimumLength(string brandName)
    {
        Assert.Equal(brandName, new Brand(brandName).BrandName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("T")]
    public void Constructor_RefusesNameThatIsBlankOrTooShort(string brandName)
    {
        Refused.With(DomainMessages.BrandNameTooShort, () => new Brand(brandName));
    }
}
