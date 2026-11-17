using Rentar.Modules.Findeks.Application;
using Rentar.Modules.Findeks.Domain;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class CustomerScoresEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Calculate_StoresTheCustomersFindeksRating()
    {
        var customer = await CreateCustomerAsync();

        var response = await Client.PostJsonAsync($"/api/v2/customers/{customer.Id}/score", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var score = await response.ReadAsAsync<CustomerScore>();
        Assert.Equal(customer.Id, score.CustomerId);
        Assert.InRange(score.FindeksRating, 0, 1900);

        var fetched = await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).ReadAsAsync<CustomerScore>();
        Assert.Equal(score.FindeksRating, fetched.FindeksRating);
    }

    [Fact]
    public async Task Calculate_Again_ReplacesTheScore()
    {
        var customer = await CreateCustomerAsync();
        await Client.PostJsonAsync($"/api/v2/customers/{customer.Id}/score", new { });

        var latest = await (await Client.PostJsonAsync($"/api/v2/customers/{customer.Id}/score", new { })).ReadAsAsync<CustomerScore>();

        var fetched = await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).ReadAsAsync<CustomerScore>();
        Assert.Equal(latest.FindeksRating, fetched.FindeksRating);
    }

    [Fact]
    public async Task Calculate_ForACustomerThatIsNotStored_IsNotFound()
    {
        var response = await Client.PostJsonAsync("/api/v2/customers/404/score", new { });

        await AssertProblemAsync(response, HttpStatusCode.NotFound, Messages.CustomerNotFound(404));
    }

    [Fact]
    public async Task CreatingACustomer_GivesItAScore()
    {
        var customer = await CreateCustomerAsync();
        await DeliverEventsAsync();

        var response = await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var score = await response.ReadAsAsync<CustomerScore>();
        Assert.Equal(customer.Id, score.CustomerId);
        Assert.InRange(score.FindeksRating, 0, 1900);
    }

    [Fact]
    public async Task Get_ForACustomerThatIsNotStored_IsNotFound()
    {
        var response = await Client.GetJsonAsync("/api/v2/customers/404/score");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Score of customer 404 was not found.");
    }
}
