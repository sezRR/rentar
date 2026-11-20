using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rentar.Api.IntegrationTests.V2;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// Opens the maintenance window on the shared host through <see cref="RentarApiFactory.Settings"/>; only the startup
/// validation needs a host of its own.
/// </summary>
public class MaintenanceTests(RentarApiFactory factory) : V2TestBase(factory)
{
    private const string Message = "Servers in Maintenance (Time)";

    [Theory]
    [InlineData("GET", "/api/v2/users")]
    [InlineData("GET", "/api/v2/users?email=driver@example.com")]
    [InlineData("GET", "/api/v2/customers")]
    [InlineData("GET", "/api/v2/customers?userId=1")]
    [InlineData("GET", "/api/v2/rentals/details")]
    [InlineData("GET", "/api/v2/cars")]
    [InlineData("GET", "/api/v2/companies")]
    [InlineData("GET", "/api/v2/findeks/rating")]
    [InlineData("POST", "/api/v2/auth/tokens")]
    [InlineData("POST", "/api/v2/users")]
    [InlineData("POST", "/api/v2/customers")]
    [InlineData("POST", "/api/v2/rentals")]
    [InlineData("POST", "/api/v2/payments")]
    [InlineData("PUT", "/api/v2/cars/1")]
    [InlineData("DELETE", "/api/v2/cars/1")]
    public async Task Maintenance_RefusesReadsAndWritesAcrossModules_BeforeAuthenticationOrBinding(string method, string path)
    {
        FreezeAt("22:30");
        EnableMaintenance();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
        {
            request.Content = new StringContent("{", Encoding.UTF8, "application/json");
        }

        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

        await AssertProblemAsync(response, HttpStatusCode.ServiceUnavailable, Message);
        Assert.Equal(TimeSpan.FromMinutes(30), response.Headers.RetryAfter?.Delta);
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Theory]
    [InlineData("GET", "/api/v1/users/getall")]
    [InlineData("GET", "/api/v1/brands/getall")]
    [InlineData("POST", "/api/v1/brands/add")]
    [InlineData("DELETE", "/api/v1/carimages/delete?id=1")]
    public async Task Maintenance_KeepsTheV1Envelope_ButAnswersServiceUnavailable(string method, string path)
    {
        FreezeAt("22:30");
        EnableMaintenance();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.ReadAsAsync<V1Response<object>>();
        Assert.False(body.Success);
        Assert.Null(body.Data);
        Assert.Equal(Message, body.Message);
        using var json = JsonDocument.Parse(await response.ReadTextAsync());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
        Assert.Equal(TimeSpan.FromMinutes(30), response.Headers.RetryAfter?.Delta);
    }

    [Theory]
    [InlineData("22:00", "23:00", "21:59:59.9999999", HttpStatusCode.OK, null)]
    [InlineData("22:00", "23:00", "22:00", HttpStatusCode.ServiceUnavailable, 3600)]
    [InlineData("22:00", "23:00", "22:59:59.5", HttpStatusCode.ServiceUnavailable, 1)]
    [InlineData("22:00", "23:00", "23:00", HttpStatusCode.OK, null)]
    [InlineData("22:00", "23:00", "00:30", HttpStatusCode.OK, null)]
    [InlineData("10:00", "10:30", "10:15", HttpStatusCode.ServiceUnavailable, 900)]
    [InlineData("10:00", "10:30", "22:30", HttpStatusCode.OK, null)]
    [InlineData("23:00", "01:00", "23:00", HttpStatusCode.ServiceUnavailable, 7200)]
    [InlineData("23:00", "01:00", "23:30", HttpStatusCode.ServiceUnavailable, 5400)]
    [InlineData("23:00", "01:00", "00:30", HttpStatusCode.ServiceUnavailable, 1800)]
    [InlineData("23:00", "01:00", "01:00", HttpStatusCode.OK, null)]
    [InlineData("23:00", "01:00", "22:30", HttpStatusCode.OK, null)]
    public async Task TheConfiguredWindow_IncludesItsStartAndExcludesItsEnd(
        string start, string end, string now, HttpStatusCode status, int? retryAfter)
    {
        FreezeAt(now);
        EnableMaintenance(start, end);

        var response = await Client.GetJsonAsync("/api/v2/brands");

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(retryAfter is { } seconds ? TimeSpan.FromSeconds(seconds) : (TimeSpan?)null, response.Headers.RetryAfter?.Delta);
    }

    [Fact]
    public async Task Maintenance_CanBeDisabled()
    {
        FreezeAt("22:30");
        Factory.Settings.Apply(("Maintenance:Enabled", "false"), ("Maintenance:StartTime", "22:00"), ("Maintenance:EndTime", "23:00"));

        var response = await Client.GetJsonAsync("/api/v2/brands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.RetryAfter);
    }

    [Fact]
    public async Task ConfigurationReload_ChangesTheGateWithoutRestartingTheHost()
    {
        FreezeAt("22:30");
        EnableMaintenance();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await Client.GetJsonAsync("/api/v2/brands")).StatusCode);

        EndMaintenance();

        Assert.Equal(HttpStatusCode.OK, (await Client.GetJsonAsync("/api/v2/brands")).StatusCode);
    }

    [Fact]
    public async Task AnEmptyWindow_IsRejectedAtStartup()
    {
        // ValidateOnStart only runs while a host starts, so this test needs one of its own.
        await using var app = WithMaintenanceWindow(start: "22:00", end: "22:00");

        Assert.Throws<OptionsValidationException>(() => app.CreateClient());
    }

    [Theory]
    [InlineData("/docs")]
    [InlineData("/openapi/v2.json")]
    [InlineData("/not-an-api")]
    [InlineData("/api/v2/not-an-endpoint")]
    public async Task Maintenance_DoesNotReplaceNonApiOrNotFoundResponses(string path)
    {
        FreezeAt("22:30");
        EnableMaintenance();

        var response = await Client.GetJsonAsync(path);

        Assert.NotEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Null(response.Headers.RetryAfter);
    }

    [Fact]
    public async Task Maintenance_DoesNotBlockStaticImages()
    {
        var car = await CreateCarAsync();
        var uploaded = await UploadImageAsync(car.Id, "car.png", TestImages.Png);
        Assert.Equal(HttpStatusCode.Created, uploaded.StatusCode);
        var image = await uploaded.ReadAsAsync<CarImageBody>();

        try
        {
            FreezeAt("22:30");
            EnableMaintenance();

            var response = await Client.GetAsync($"/uploads/{image.ImagePath}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(TestImages.Png, await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            Assert.Null(response.Headers.RetryAfter);
        }
        finally
        {
            EndMaintenance();
            await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
        }
    }

    [Fact]
    public async Task Maintenance_DoesNotBlockCorsPreflight()
    {
        FreezeAt("22:30");
        EnableMaintenance();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v2/brands");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin"));
        Assert.Null(response.Headers.RetryAfter);
    }

    [Fact]
    public async Task ARefusedWrite_DoesNotChangeStoredData()
    {
        FreezeAt("22:30");
        EnableMaintenance();

        var response = await Client.PostJsonAsync("/api/v2/brands", new { brandName = "Toyota" });

        await AssertProblemAsync(response, HttpStatusCode.ServiceUnavailable, Message);
        EndMaintenance();
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/brands")).ReadItemsAsync<BrandBody>());
    }

    [Fact]
    public async Task Maintenance_DoesNotPreventEventDelivery()
    {
        FreezeAt("22:30");
        var customer = await CreateCustomerAsync();
        EnableMaintenance();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).StatusCode);

        await DeliverEventsAsync();

        Assert.True(Assert.Single(await Factory.OutboxAsync("customers")).Processed);
        EndMaintenance();
        Assert.Equal(HttpStatusCode.OK, (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).StatusCode);
    }

    /// <summary>Closes the window again, so the test can look at what is stored through the same client.</summary>
    private void EndMaintenance()
    {
        Factory.Settings.Apply(("Maintenance:Enabled", "false"));
    }

    private WebApplicationFactory<Program> WithMaintenanceWindow(string start, string end)
    {
        return Factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Maintenance:Enabled"] = "true",
                ["Maintenance:StartTime"] = start,
                ["Maintenance:EndTime"] = end
            })));
    }

    private void FreezeAt(string time)
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.Add(TimeOnly.Parse(time).ToTimeSpan())));
    }
}
