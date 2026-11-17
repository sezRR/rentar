using Rentar.Modules.Companies.Domain;
using System.Collections.Generic;
using Xunit;

namespace Rentar.Domain.UnitTests.Companies;

public class EmployeeTests
{
    // A role not yet stored belongs to no company (its CompanyId is 0), so an employee of company 0 may hold it and an
    // employee of any other company may not.
    private const int RolesCompanyId = 0;

    [Fact]
    public void Constructor_KeepsWhatItWasGiven()
    {
        var admin = new Role("Admin", ["employees.manage"], isPredefined: true);
        var member = new Role("Member", [], isPredefined: true);

        var employee = new Employee(RolesCompanyId, userId: 9, [admin, member]);

        Assert.Equal(RolesCompanyId, employee.CompanyId);
        Assert.Equal(9, employee.UserId);
        Assert.Equal([admin, member], employee.Roles);
    }

    [Fact]
    public void Constructor_RefusesEmployeeWithoutRoles()
    {
        Refused.With(DomainMessages.EmployeeNeedsRole, () => new Employee(RolesCompanyId, userId: 9, []));
    }

    [Fact]
    public void Constructor_RefusesRoleOfAnotherCompany()
    {
        var role = new Role("Member", [], isPredefined: true);

        Refused.With(DomainMessages.EmployeeRoleOfOtherCompany, () => new Employee(companyId: 5, userId: 9, [role]));
    }

    [Fact]
    public void Constructor_ReadsTheRolesOnce()
    {
        var reads = 0;

        IEnumerable<Role> Roles()
        {
            reads++;
            yield return new Role("Member", [], isPredefined: true);
        }

        var employee = new Employee(RolesCompanyId, userId: 9, Roles());

        Assert.Single(employee.Roles);
        Assert.Equal(1, reads);
    }

    [Fact]
    public void Permissions_AreEveryPermissionItsRolesGrant_Once()
    {
        var admin = new Role("Admin", ["employees.manage", "rentals.read"], isPredefined: true);
        var reader = new Role("Reader", ["rentals.read"], isPredefined: false);

        var employee = new Employee(RolesCompanyId, userId: 9, [admin, reader]);

        Assert.Equal(new HashSet<string> { "employees.manage", "rentals.read" }, employee.Permissions());
    }

    [Fact]
    public void HasPermission_IsWhetherOneOfItsRolesGrantsIt()
    {
        var employee = new Employee(RolesCompanyId, userId: 9, [new Role("Reader", ["rentals.read"], isPredefined: false)]);

        Assert.True(employee.HasPermission("rentals.read"));
        Assert.False(employee.HasPermission("employees.manage"));
    }
}
