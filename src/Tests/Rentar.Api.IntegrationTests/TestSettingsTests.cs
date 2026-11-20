using Rentar.Api.IntegrationTests.V2;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// Guards the switch the maintenance and seeding tests rely on: what a test applies reaches the shared host's next
/// request, and the reset between tests takes it away again.
/// </summary>
public class TestSettingsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Settings_OverrideTheFactoryDefaults_AndResetBetweenTests()
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(22).AddMinutes(30)));
        EnableMaintenance();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await Client.GetJsonAsync("/api/v2/brands")).StatusCode);

        await Factory.ResetAsync();
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(22).AddMinutes(30)));

        Assert.Equal(HttpStatusCode.OK, (await Client.GetJsonAsync("/api/v2/brands")).StatusCode);
    }
}
