using Rentar.Modules.Companies.Domain;
using System.Linq;
using Xunit;

namespace Rentar.Domain.UnitTests.Companies;

public class CompanyTests
{
    [Fact]
    public void Constructor_KeepsItsRoles_AndMakesTheOwnerItsOnlyEmployee()
    {
        var owner = new Role("Owner", ["company.manage", "employees.manage"], isPredefined: true);
        var member = new Role("Member", [], isPredefined: true);

        var company = new Company("Acme", [owner, member], ownerUserId: 9, owner);

        Assert.Equal("Acme", company.Name);
        Assert.Equal([owner, member], company.Roles);
        var employee = Assert.Single(company.Employees);
        Assert.Equal(9, employee.UserId);
        Assert.Same(owner, Assert.Single(employee.Roles));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_RefusesBlankName(string name)
    {
        var owner = new Role("Owner", [], isPredefined: true);

        Refused.With(DomainMessages.CompanyNameMissing, () => new Company(name, [owner], ownerUserId: 9, owner));
    }

    [Fact]
    public void Constructor_RefusesOwnerRoleThatIsNotOneOfItsRoles()
    {
        var owner = new Role("Owner", [], isPredefined: true);
        var elsewhere = new Role("Owner", [], isPredefined: true);

        Refused.With(DomainMessages.CompanyOwnerRoleNotOfCompany, () => new Company("Acme", [owner], ownerUserId: 9, elsewhere));
    }

    [Fact]
    public void Constructor_RefusesBlankNameBeforeLookingAtTheOwnerRole()
    {
        var elsewhere = new Role("Owner", [], isPredefined: true);

        Refused.With(DomainMessages.CompanyNameMissing, () => new Company(" ", [], ownerUserId: 9, elsewhere));
    }

    [Fact]
    public void Constructor_CopiesTheRolesItWasGiven()
    {
        var owner = new Role("Owner", [], isPredefined: true);
        var roles = new[] { owner, new Role("Member", [], isPredefined: true) }.ToList();

        var company = new Company("Acme", roles, ownerUserId: 9, owner);
        roles.Clear();

        Assert.Equal(2, company.Roles.Count);
    }

    [Fact]
    public void Renamed_IsTheSameCompanyUnderTheNewName()
    {
        var company = NewCompany();

        var renamed = company.Renamed("Acme Rentals");

        Assert.Equal(company.Id, renamed.Id);
        Assert.Equal("Acme Rentals", renamed.Name);
        Assert.Equal("Acme", company.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Renamed_RefusesBlankName(string name)
    {
        var company = NewCompany();

        Refused.With(DomainMessages.CompanyNameMissing, () => company.Renamed(name));
    }

    private static Company NewCompany()
    {
        var owner = new Role("Owner", [], isPredefined: true);
        return new Company("Acme", [owner], ownerUserId: 9, owner);
    }
}
