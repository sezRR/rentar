using Rentar.Modules.Customers.Application;
using Rentar.Modules.Customers.Application.Commands;
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

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.ReadAsAsync<CustomerBody>();
        Assert.Equal($"/api/v2/customers/{customer.Id}", response.Headers.Location?.OriginalString);

        var fetched = await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}")).ReadAsAsync<CustomerBody>();
        Assert.Equal(user.Id, fetched.UserId);
        Assert.Null(fetched.CompanyId);
    }

    [Fact]
    public async Task Create_ForACompanyTheUserWorksAt_MakesItOneOfTheCompanysCustomers()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();

        var customer = await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(owner.Id, company.Id));

        Assert.Equal(company.Id, customer.CompanyId);
    }

    [Fact]
    public async Task Create_ForACompanyTheUserDoesNotWorkAt_IsABadRequest()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var outsider = await CreateUserAsync("outsider@example.com");

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(outsider.Id, company.Id));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotEmployeeOfCompany);
    }

    [Fact]
    public async Task Create_ForACompanyThatIsNotStored_IsABadRequest()
    {
        var user = await CreateUserAsync("renter@example.com");

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id, 404));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotEmployeeOfCompany);
    }

    [Fact]
    public async Task Create_ASecondCustomerForTheSameUser_IsAConflict()
    {
        FreezeClockAtNoon();
        var customer = await CreateCustomerAsync();

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(customer.UserId));

        await AssertProblemAsync(response, HttpStatusCode.Conflict, Messages.UserAlreadyCustomer);
        Assert.Equal([customer.UserId], await UserIdsAsync("/api/v2/customers"));
    }

    [Fact]
    public async Task Create_ForAUserThatIsNotStored_IsABadRequest()
    {
        FreezeClockAtNoon();

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(404));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotFound);
        Assert.Empty(await UserIdsAsync("/api/v2/customers"));
    }

    [Fact]
    public async Task Create_ForAnInactiveUser_IsABadRequest()
    {
        FreezeClockAtNoon();
        var user = await CreateUserAsync("inactive@example.com");
        await DeactivateUserAsync(user.Id);

        var response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.UserNotActive);
        Assert.Empty(await UserIdsAsync("/api/v2/customers"));
    }

    [Fact]
    public async Task GetAll_FiltersByUser()
    {
        FreezeClockAtNoon();
        var first = await CreateCustomerAsync("first@example.com");
        var second = await CreateCustomerAsync("second@example.com");

        Assert.Equal([first.UserId, second.UserId], await UserIdsAsync("/api/v2/customers"));
        Assert.Equal([second.UserId], await UserIdsAsync($"/api/v2/customers?userId={second.UserId}"));
    }

    [Fact]
    public async Task UnknownCustomer_IsNotFound()
    {
        var response = await Client.GetJsonAsync("/api/v2/customers/404");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Customer 404 was not found.");
    }

    private async Task<List<int>> UserIdsAsync(string url)
    {
        var response = await Client.GetJsonAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customers = await response.ReadAsAsync<List<CustomerBody>>();
        return customers.Select(c => c.UserId).Order().ToList();
    }
}
