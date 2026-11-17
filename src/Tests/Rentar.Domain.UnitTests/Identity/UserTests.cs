using Rentar.Modules.Identity.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Identity;

public class UserTests
{
    private static readonly byte[] Hash = [1, 2, 3];
    private static readonly byte[] Salt = [4, 5, 6];

    [Fact]
    public void Constructor_KeepsWhatItWasGiven()
    {
        var user = new User("Ada", "Lovelace", "user@example.com", Hash, Salt, status: true);

        Assert.Equal("Ada", user.FirstName);
        Assert.Equal("Lovelace", user.LastName);
        Assert.Equal("user@example.com", user.Email);
        Assert.Same(Hash, user.PasswordHash);
        Assert.Same(Salt, user.PasswordSalt);
        Assert.True(user.Status);
    }

    [Theory]
    [InlineData(null, "Lovelace")]
    [InlineData("", "Lovelace")]
    [InlineData("  ", "Lovelace")]
    [InlineData("Ada", null)]
    [InlineData("Ada", "")]
    [InlineData("Ada", "  ")]
    public void Constructor_RefusesBlankFirstOrLastName(string firstName, string lastName)
    {
        Refused.With(DomainMessages.UserNameMissing, () => NewUser(firstName, lastName));
    }

    [Theory]
    [InlineData("a@b")]
    [InlineData("user@example.com")]
    [InlineData("first.last+tag@sub.example.co")]
    public void Constructor_TakesEmailWithOneAtNeitherFirstNorLast(string email)
    {
        Assert.Equal(email, NewUser(email: email).Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("user.example.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@@example.com")]
    [InlineData("user@example@com")]
    public void Constructor_RefusesEmailThatIsNotAnAddress(string email)
    {
        Refused.With(DomainMessages.UserEmailInvalid, () => NewUser(email: email));
    }

    [Fact]
    public void Constructor_RefusesBlankNameBeforeLookingAtTheEmail()
    {
        Refused.With(DomainMessages.UserNameMissing, () => NewUser(firstName: " ", email: "not an address"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithDetails_ChangesTheDetails_AndKeepsWhetherTheUserIsActive(bool status)
    {
        var user = NewUser(status: status);
        byte[] newHash = [7], newSalt = [8];

        var changed = user.WithDetails("Grace", "Hopper", "grace@example.com", newHash, newSalt);

        Assert.Equal(user.Id, changed.Id);
        Assert.Equal("Grace", changed.FirstName);
        Assert.Equal("Hopper", changed.LastName);
        Assert.Equal("grace@example.com", changed.Email);
        Assert.Same(newHash, changed.PasswordHash);
        Assert.Same(newSalt, changed.PasswordSalt);
        Assert.Equal(status, changed.Status);
        Assert.Equal("Ada", user.FirstName);
    }

    [Fact]
    public void WithDetails_RefusesBlankName()
    {
        var user = NewUser();

        Refused.With(DomainMessages.UserNameMissing, () => user.WithDetails("", "Hopper", "grace@example.com", Hash, Salt));
    }

    [Fact]
    public void WithDetails_RefusesEmailThatIsNotAnAddress()
    {
        var user = NewUser();

        Refused.With(DomainMessages.UserEmailInvalid, () => user.WithDetails("Grace", "Hopper", "grace.example.com", Hash, Salt));
    }

    [Fact]
    public void Deactivated_IsTheSameUser_Inactive()
    {
        var user = NewUser(status: true);

        var deactivated = user.Deactivated();

        Assert.False(deactivated.Status);
        Assert.Equal(user.Id, deactivated.Id);
        Assert.Equal(user.FirstName, deactivated.FirstName);
        Assert.Equal(user.LastName, deactivated.LastName);
        Assert.Equal(user.Email, deactivated.Email);
        Assert.Same(user.PasswordHash, deactivated.PasswordHash);
        Assert.Same(user.PasswordSalt, deactivated.PasswordSalt);
        Assert.True(user.Status);
    }

    [Fact]
    public void Deactivated_StaysInactive()
    {
        Assert.False(NewUser(status: false).Deactivated().Status);
    }

    private static User NewUser(
        string firstName = "Ada",
        string lastName = "Lovelace",
        string email = "user@example.com",
        bool status = true)
    {
        return new User(firstName, lastName, email, Hash, Salt, status);
    }
}
