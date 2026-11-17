using Rentar.Modules.Findeks.Presentation.Endpoints.V2;
using Rentar.Modules.Payments.Application;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class PaymentsAndFindeksEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Payment_ThatGoesThrough_AnswersNoContent()
    {
        var response = await Client.PostJsonAsync("/api/v2/payments", new { });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Payment_ThatIsDeclined_AnswersPaymentRequired()
    {
        Factory.PaymentGateway.Declines = true;

        var response = await Client.PostJsonAsync("/api/v2/payments", new { });

        await AssertProblemAsync(response, HttpStatusCode.PaymentRequired, Messages.PaymentFailed);
    }

    [Fact]
    public async Task FindeksRating_IsReadWithGet()
    {
        var response = await Client.GetJsonAsync("/api/v2/findeks/rating");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rating = await response.ReadAsAsync<FindeksRatingResponse>();
        Assert.InRange(rating.Rating, 0, 1900);
    }
}
