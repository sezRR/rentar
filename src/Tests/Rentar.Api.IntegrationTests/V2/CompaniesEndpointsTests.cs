using Rentar.Modules.Companies.Application;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Domain;
using Rentar.Modules.Companies.Presentation.Endpoints.V2;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class CompaniesEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Register_WithoutAnAccessToken_IsUnauthorized()
    {
        var response = await Client.PostJsonAsync("/api/v2/companies", new CompanyRequest("Rentar Logistics"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_AnswersCreated_AndMakesTheCallerItsOwner()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");

        var response = await Client.PostJsonAsync("/api/v2/companies", new CompanyRequest("Rentar Logistics"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var company = await response.ReadAsAsync<Company>();
        Assert.Equal("Rentar Logistics", company.Name);
        Assert.Equal($"/api/v2/companies/{company.Id}", response.Headers.Location?.OriginalString);

        var employees = await (await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/employees")).ReadAsAsync<List<EmployeeResponse>>();
        var employee = Assert.Single(employees);
        Assert.Equal(owner.Id, employee.UserId);
        Assert.Equal("Sezer", employee.FirstName);
        Assert.Equal(["Owner"], employee.Roles.Select(role => role.Name));
    }

    [Fact]
    public async Task Register_GivesTheCompanyThePredefinedRoles()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();

        var roles = await (await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/roles")).ReadAsAsync<List<Role>>();

        Assert.Equal(["Owner", "Admin", "Member"], roles.Select(role => role.Name));
        Assert.All(roles, role => Assert.True(role.IsPredefined));
        Assert.All(roles, role => Assert.Equal(company.Id, role.CompanyId));
        Assert.Equal(CompanyPermissions.All, roles[0].Permissions);
        Assert.Equal(
            [CompanyPermissions.EmployeesRead, CompanyPermissions.EmployeesManage, CompanyPermissions.RentalsRead],
            roles[1].Permissions);
        Assert.Empty(roles[2].Permissions);
    }

    [Fact]
    public async Task Register_WithAnEmptyName_ReturnsValidationProblem()
    {
        await SignInAsNewUserAsync("owner@example.com");

        var response = await Client.PostJsonAsync("/api/v2/companies", new CompanyRequest(""));

        await AssertValidationProblemAsync(response, "name");
    }

    [Fact]
    public async Task Register_ForAnInactiveUser_IsABadRequest()
    {
        var user = await SignInAsNewUserAsync("owner@example.com");
        await Factory.DeactivateUserAsync(user.Id);

        var response = await Client.PostJsonAsync("/api/v2/companies", new CompanyRequest("Rentar Logistics"));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotActive);
    }

    [Fact]
    public async Task GetMine_ListsOnlyTheCompaniesTheCallerWorksAt()
    {
        await SignInAsNewUserAsync("first@example.com");
        var first = await RegisterCompanyAsync("First Company");
        await SignInAsNewUserAsync("second@example.com");
        var second = await RegisterCompanyAsync("Second Company");

        var companies = await (await Client.GetJsonAsync("/api/v2/companies")).ReadAsAsync<List<Company>>();

        Assert.Equal([second.Id], companies.Select(company => company.Id));
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task Get_ForAnEmployee_AnswersTheCompany()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync("Rentar Logistics");
        var member = await CreateUserAsync("member@example.com");
        await AddEmployeeAsync(company.Id, member.Id, "Member");
        await SignInAsync("member@example.com");

        var fetched = await (await Client.GetJsonAsync($"/api/v2/companies/{company.Id}")).ReadAsAsync<Company>();

        Assert.Equal("Rentar Logistics", fetched.Name);
    }

    [Fact]
    public async Task Get_ForSomeoneWhoDoesNotWorkThere_IsNotFound_AsIfItDidNotExist()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        await SignInAsNewUserAsync("outsider@example.com");

        var response = await Client.GetJsonAsync($"/api/v2/companies/{company.Id}");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, $"Company {company.Id} was not found.");
        await AssertProblemAsync(await Client.GetJsonAsync("/api/v2/companies/404"), HttpStatusCode.NotFound, "Company 404 was not found.");
        await AssertProblemAsync(await Client.GetJsonAsync($"/api/v2/companies/{company.Id}/roles"), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ByTheOwner_RenamesTheCompany()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync("Rentar Logistics");

        var response = await Client.PutJsonAsync($"/api/v2/companies/{company.Id}", new CompanyRequest("Rentar Fleet"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await (await Client.GetJsonAsync($"/api/v2/companies/{company.Id}")).ReadAsAsync<Company>();
        Assert.Equal("Rentar Fleet", fetched.Name);
    }

    [Fact]
    public async Task Update_ByAnAdmin_IsForbidden()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync("Rentar Logistics");
        var admin = await CreateUserAsync("admin@example.com");
        await AddEmployeeAsync(company.Id, admin.Id, "Admin");
        await SignInAsync("admin@example.com");

        var response = await Client.PutJsonAsync($"/api/v2/companies/{company.Id}", new CompanyRequest("Rentar Fleet"));

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, Messages.PermissionMissing(CompanyPermissions.CompanyManage));
        var fetched = await (await Client.GetJsonAsync($"/api/v2/companies/{company.Id}")).ReadAsAsync<Company>();
        Assert.Equal("Rentar Logistics", fetched.Name);
    }
}
