using Rentar.Api.IntegrationTests.V2;
using Rentar.Modules.Companies.Presentation.Endpoints.V2;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// The behavior of use cases only v1 exposes. Everything v1 shares with v2 is tested through v2, and the v1 responses
/// themselves are pinned by <see cref="V1ContractTests"/>. A test here moves to v2 once its use case gets a v2 route.
/// </summary>
public class V1OnlyUseCasesTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Register_AnswersAUsableAccessToken()
    {
        var response = await Client.PostJsonAsync("/api/v1/auth/register", new RegisterUserCommand("Sezer", "Tetik", "driver@example.com", Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = (await response.ReadAsAsync<V1Response<AccessToken>>()).Data;
        Assert.Equal(3, token.Token.Split('.').Length);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        var registered = await Client.PostJsonAsync("/api/v2/companies", new CompanyRequest("Rentar Logistics"));
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
    }

    [Fact]
    public async Task GetRentalByCarId_AnswersTheRentalOfThatCar()
    {
        var firstCar = await CreateCarAsync(description: "Corolla 1.6");
        var secondCar = await CreateCarAsync("Fiat", "White", "Egea");
        var first = await CreateCustomerAsync("first@example.com");
        var second = await CreateCustomerAsync("second@example.com");
        var rentDate = new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
        await CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(firstCar.Id, first.Id, rentDate, rentDate.AddDays(2)));
        await CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(secondCar.Id, second.Id, rentDate, rentDate.AddDays(2)));

        var response = await Client.GetJsonAsync($"/api/v1/rentals/getrentalbycarid?carId={secondCar.Id}");

        var rental = (await response.ReadAsAsync<V1Response<RentalBody>>()).Data;
        Assert.Equal(secondCar.Id, rental.CarId);
        Assert.Equal(second.Id, rental.CustomerId);
    }
}
