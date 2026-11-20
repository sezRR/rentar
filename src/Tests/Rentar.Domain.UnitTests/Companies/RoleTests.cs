using Rentar.Modules.Companies.Domain;
using System.Collections.Generic;
using Xunit;

namespace Rentar.Domain.UnitTests.Companies;

public class RoleTests
{
    [Fact]
    public void Constructor_KeepsWhatItWasGiven()
    {
        var role = new Role("Admin", ["employees.manage", "rentals.read"], isPredefined: true);

        Assert.Equal("Admin", role.Name);
        Assert.Equal(["employees.manage", "rentals.read"], role.Permissions);
        Assert.True(role.IsPredefined);
    }

    [Fact]
    public void Constructor_TakesRoleThatGrantsNothing()
    {
        Assert.Empty(new Role("Guest", [], isPredefined: false).Permissions);
    }

    [Fact]
    public void Constructor_CopiesThePermissionsItWasGiven()
    {
        var permissions = new List<string> { "rentals.read" };

        var role = new Role("Reader", permissions, isPredefined: false);
        permissions.Add("employees.manage");

        Assert.Equal(["rentals.read"], role.Permissions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_RefusesBlankName(string name)
    {
        Refused.With(DomainMessages.RoleNameMissing, () => new Role(name, [], isPredefined: false));
    }
}
