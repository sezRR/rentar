using Rentar.Modules.Customers.Application;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class CustomersEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Create_AnswersCreated_WithTheCustomerAndWhereItLives()
    {
        var user = await CreateUserAsync("renter@example.com");

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id, "Rentar Logistics"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.ReadAsAsync<Customer>();
        Assert.Equal($"/api/v2/customers/{customer.Id}", response.Headers.Location?.OriginalString);

        var fetched = await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}")).ReadAsAsync<Customer>();
        Assert.Equal(user.Id, fetched.UserId);
        Assert.Equal("Rentar Logistics", fetched.CompanyName);
    }

    [Fact]
    public async Task Create_WithoutACompany_StoresNoCompanyNameOrCompany()
    {
        var user = await CreateUserAsync("renter@example.com");

        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(user.Id));

        Assert.Null(customer.CompanyName);
        Assert.Null(customer.CompanyId);
        Assert.Null((await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}")).ReadAsAsync<Customer>()).CompanyName);
    }

    [Fact]
    public async Task Create_WithABlankCompanyName_StoresNone()
    {
        var user = await CreateUserAsync("renter@example.com");

        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(user.Id, "   "));

        Assert.Null(customer.CompanyName);
    }

    [Fact]
    public async Task Create_ForACompanyTheUserWorksAt_TakesTheCompanysName()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync("Rentar Logistics");

        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(owner.Id, CompanyId: company.Id));

        Assert.Equal(company.Id, customer.CompanyId);
        Assert.Equal("Rentar Logistics", customer.CompanyName);
    }

    [Fact]
    public async Task Create_ForACompany_KeepsTheNameItIsGiven()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync("Rentar Logistics");

        var customer = await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(owner.Id, "Rentar Istanbul Office", company.Id));

        Assert.Equal(company.Id, customer.CompanyId);
        Assert.Equal("Rentar Istanbul Office", customer.CompanyName);
    }

    [Fact]
    public async Task Create_ForACompanyTheUserDoesNotWorkAt_IsABadRequest()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var outsider = await CreateUserAsync("outsider@example.com");

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(outsider.Id, CompanyId: company.Id));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotEmployeeOfCompany);
    }

    [Fact]
    public async Task Create_ForACompanyThatIsNotStored_IsABadRequest()
    {
        var user = await CreateUserAsync("renter@example.com");

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id, CompanyId: 404));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotEmployeeOfCompany);
    }

    [Fact]
    public async Task Create_ForAUserThatIsNotStored_IsABadRequest()
    {
        FreezeClockAtNoon();

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(404, "Rentar Logistics"));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotFound);
        Assert.Empty(await CompanyNamesAsync("/api/v2/customers"));
    }

    [Fact]
    public async Task Create_ForAnInactiveUser_IsABadRequest()
    {
        FreezeClockAtNoon();
        var user = await CreateUserAsync("inactive@example.com");
        await Factory.DeactivateUserAsync(user.Id);

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id, "Rentar Logistics"));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotActive);
        Assert.Empty(await CompanyNamesAsync("/api/v2/customers"));
    }

    [Fact]
    public async Task GetAll_FiltersByUser()
    {
        FreezeClockAtNoon();
        var first = await CreateUserAsync("first@example.com");
        var second = await CreateUserAsync("second@example.com");
        await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(first.Id, "First Company"));
        await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(second.Id, "Second Company"));
        await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(second.Id, "Third Company"));

        Assert.Equal(["First Company", "Second Company", "Third Company"], await CompanyNamesAsync("/api/v2/customers"));
        Assert.Equal(["Second Company", "Third Company"], await CompanyNamesAsync($"/api/v2/customers?userId={second.Id}"));
    }

    [Fact]
    public async Task GetAll_DuringTheMaintenanceHour_IsServiceUnavailable()
    {
        FreezeClockInMaintenanceHour();

        var response = await Client.GetJsonAsync("/api/v2/customers");

        await AssertProblemAsync(response, HttpStatusCode.ServiceUnavailable, Messages.MaintenanceTime);
    }

    [Fact]
    public async Task Create_WithCompanyNameShorterThanFiveCharacters_ReturnsValidationProblem()
    {
        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(1, "Acme"));

        await AssertValidationProblemAsync(response, "companyName");
    }

    [Fact]
    public async Task UnknownCustomer_IsNotFound()
    {
        var response = await Client.GetJsonAsync("/api/v2/customers/404");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Customer 404 was not found.");
    }

    private async Task<List<string>> CompanyNamesAsync(string url)
    {
        var response = await Client.GetJsonAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customers = await response.ReadAsAsync<List<Customer>>();
        return customers.Select(c => c.CompanyName).Order().ToList();
    }
}
