using Rentar.Modules.Fleet.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Fleet;

public class ColorTests
{
    [Theory]
    [InlineData("Re")]
    [InlineData("Red")]
    public void Constructor_TakesNameOfAtLeastTheMinimumLength(string colorName)
    {
        Assert.Equal(colorName, new Color(colorName).ColorName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("R")]
    public void Constructor_RefusesNameThatIsBlankOrTooShort(string colorName)
    {
        Refused.With(DomainMessages.ColorNameInvalid, () => new Color(colorName));
    }

    [Fact]
    public void RefusalMessage_IsTheOneV1HasAlwaysAnswered()
    {
        Assert.Equal("Color Name is Invalid", DomainMessages.ColorNameInvalid);
    }
}
