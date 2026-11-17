using Rentar.Modules.Companies.Application;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Presentation.Endpoints.V2;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class EmployeesEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Add_ByTheOwner_AnswersCreated_WithTheEmployeeAndWhereItLives()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var user = await CreateUserAsync("admin@example.com", firstName: "Ada");
        var adminRoleId = await RoleIdAsync(company.Id, "Admin");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(user.Id, [adminRoleId]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var employee = await response.ReadAsAsync<EmployeeResponse>();
        Assert.Equal($"/api/v2/companies/{company.Id}/employees/{employee.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal(user.Id, employee.UserId);
        Assert.Equal(company.Id, employee.CompanyId);
        Assert.Equal("Ada", employee.FirstName);
        Assert.Equal(["Admin"], employee.Roles.Select(role => role.Name));

        Assert.Equal(["Sezer", "Ada"], (await EmployeesAsync(company.Id)).Select(e => e.FirstName));
    }

    [Fact]
    public async Task Add_ByAnAdmin_CanGrantTheRolesItsPermissionsCover()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var admin = await CreateUserAsync("admin@example.com");
        await AddEmployeeAsync(company.Id, admin.Id, "Admin");
        await SignInAsync("admin@example.com");
        var member = await CreateUserAsync("member@example.com");
        var other = await CreateUserAsync("other-admin@example.com");

        await AddEmployeeAsync(company.Id, member.Id, "Member");
        await AddEmployeeAsync(company.Id, other.Id, "Admin");

        Assert.Equal(4, (await EmployeesAsync(company.Id)).Count);
    }

    [Fact]
    public async Task Add_ByAnAdmin_CanNotGrantTheOwnerRole()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var admin = await CreateUserAsync("admin@example.com");
        await AddEmployeeAsync(company.Id, admin.Id, "Admin");
        var ownerRoleId = await RoleIdAsync(company.Id, "Owner");
        await SignInAsync("admin@example.com");
        var user = await CreateUserAsync("new@example.com");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(user.Id, [ownerRoleId]));

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.CannotGrantMissingPermissions);
        Assert.Equal(2, (await EmployeesAsync(company.Id)).Count);
    }

    [Fact]
    public async Task Add_ByAMember_IsForbidden()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        await AddEmployeeAsync(company.Id, member.Id, "Member");
        var memberRoleId = await RoleIdAsync(company.Id, "Member");
        await SignInAsync("member@example.com");
        var user = await CreateUserAsync("new@example.com");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(user.Id, [memberRoleId]));

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.PermissionMissing(CompanyPermissions.EmployeesManage));
    }

    [Fact]
    public async Task Add_AUserWhoAlreadyWorksThere_IsAConflict()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var memberRoleId = await RoleIdAsync(company.Id, "Member");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(owner.Id, [memberRoleId]));

        await AssertProblemAsync(response, HttpStatusCode.Conflict, Messages.UserAlreadyEmployee);
    }

    [Fact]
    public async Task Add_AUserThatIsNotStored_IsABadRequest()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var memberRoleId = await RoleIdAsync(company.Id, "Member");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(404, [memberRoleId]));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotFound);
    }

    [Fact]
    public async Task Add_WithARoleOfAnotherCompany_IsABadRequest()
    {
        await SignInAsNewUserAsync("other@example.com");
        var other = await RegisterCompanyAsync("Other Company");
        var foreignRoleId = await RoleIdAsync(other.Id, "Member");
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var user = await CreateUserAsync("new@example.com");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(user.Id, [foreignRoleId]));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.RoleNotFound);
    }

    [Fact]
    public async Task Add_WithoutRoles_ReturnsValidationProblem()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var user = await CreateUserAsync("new@example.com");

        var response = await Client.PostJsonAsync($"/api/v2/companies/{company.Id}/employees", new EmployeeRequest(user.Id, []));

        await AssertValidationProblemAsync(response, "roleIds");
    }

    [Fact]
    public async Task GetAll_ByAMember_IsForbidden()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        await AddEmployeeAsync(company.Id, member.Id, "Member");
        await SignInAsync("member@example.com");

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/employees");

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.PermissionMissing(CompanyPermissions.EmployeesRead));
    }

    [Fact]
    public async Task Remove_ByTheOwner_RemovesTheEmployee()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        var employee = await AddEmployeeAsync(company.Id, member.Id, "Member");

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.DoesNotContain(employee.Id, (await EmployeesAsync(company.Id)).Select(e => e.Id));
    }

    [Fact]
    public async Task Remove_TheLastOwner_IsABadRequest()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var owner = Assert.Single(await EmployeesAsync(company.Id));

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{owner.Id}");

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CompanyNeedsFullAccessEmployee);
        Assert.Single(await EmployeesAsync(company.Id));
    }

    [Fact]
    public async Task Remove_AnOwner_WhileAnotherOwnerStays_LetsTheOwnerLeave()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var first = Assert.Single(await EmployeesAsync(company.Id));
        var second = await CreateUserAsync("second-owner@example.com");
        await AddEmployeeAsync(company.Id, second.Id, "Owner");

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{first.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await AssertProblemAsync(await Client.GetJsonAsync($"/api/v2/companies/{company.Id}"), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_AnOwnerByAnAdmin_IsForbidden()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var owner = Assert.Single(await EmployeesAsync(company.Id));
        var admin = await CreateUserAsync("admin@example.com");
        await AddEmployeeAsync(company.Id, admin.Id, "Admin");
        await SignInAsync("admin@example.com");

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{owner.Id}");

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.CannotRemoveEmployeeWithMorePermissions);
    }

    [Fact]
    public async Task Remove_AnEmployeeOfAnotherCompany_IsNotFound()
    {
        await SignInAsNewUserAsync("other@example.com");
        var other = await RegisterCompanyAsync("Other Company");
        var foreignEmployee = Assert.Single(await EmployeesAsync(other.Id));
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{foreignEmployee.Id}");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, Messages.EmployeeNotFound(foreignEmployee.Id));
    }

    [Fact]
    public async Task Remove_AnEmployeeWhoRentsForTheCompany_MakesTheirCustomerOneWithNoCompany()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        var employee = await AddEmployeeAsync(company.Id, member.Id, "Member");
        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(member.Id, company.Id));

        var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(company.Id, (await CustomerAsync(customer.Id)).CompanyId);

        await DeliverEventsAsync();

        Assert.Null((await CustomerAsync(customer.Id)).CompanyId);
    }

    [Fact]
    public async Task Remove_FromAnotherCompany_KeepsTheCompanyTheCustomerRentsFor()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var rentingFor = await RegisterCompanyAsync();
        var other = await RegisterCompanyAsync("Other Company");
        var member = await CreateUserAsync("member@example.com");
        await AddEmployeeAsync(rentingFor.Id, member.Id, "Member");
        var otherEmployment = await AddEmployeeAsync(other.Id, member.Id, "Member");
        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(member.Id, rentingFor.Id));

        await Client.DeleteResourceAsync($"/api/v2/companies/{other.Id}/employees/{otherEmployment.Id}");
        await DeliverEventsAsync();

        Assert.Equal(rentingFor.Id, (await CustomerAsync(customer.Id)).CompanyId);
    }

    [Fact]
    public async Task Remove_WhileTheCustomerCanNotBeUpdated_RemovesTheEmployee_AndClearsTheCompanyOnARetry()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        var employee = await AddEmployeeAsync(company.Id, member.Id, "Member");
        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(member.Id, company.Id));

        // Customers reacts after the removal committed, so its failure leaves the removal in place and is retried.
        await using (await Factory.RefuseAsync("customers", "Customers", "UPDATE"))
        {
            var response = await Client.DeleteResourceAsync($"/api/v2/companies/{company.Id}/employees/{employee.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            await DeliverEventsAsync();
        }

        Assert.DoesNotContain(employee.Id, (await EmployeesAsync(company.Id)).Select(e => e.Id));
        Assert.Equal(company.Id, (await CustomerAsync(customer.Id)).CompanyId);

        Factory.Clock.FreezeAt(DateTimeOffset.UtcNow.AddMinutes(1));
        await DeliverEventsAsync();

        Assert.Null((await CustomerAsync(customer.Id)).CompanyId);
    }

    private async Task<Customer> CustomerAsync(int customerId)
    {
        var response = await Client.GetJsonAsync($"/api/v2/customers/{customerId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsAsync<Customer>();
    }

    private async Task<List<EmployeeResponse>> EmployeesAsync(int companyId)
    {
        var response = await Client.GetJsonAsync($"/api/v2/companies/{companyId}/employees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsAsync<List<EmployeeResponse>>();
    }
}
