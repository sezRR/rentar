using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Companies.Presentation.Endpoints.V2;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Fleet.Infrastructure.Storage;
using Rentar.Modules.Fleet.Presentation.Endpoints.V2;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Presentation.Endpoints.V2;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

/// <summary>
/// Creates test data through the v2 API itself and checks its problem responses.
/// </summary>
public abstract class V2TestBase(RentarApiFactory factory) : IntegrationTestBase(factory)
{
    protected const string Password = "Passw0rd!";

    /// <summary>
    /// Pins local time to midday for tests that need a known instant.
    /// </summary>
    protected void FreezeClockAtNoon()
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(12)));
    }

    /// <summary>
    /// Turns the daily maintenance window on for the shared host, in server-local time, until the test ends. Freeze the
    /// clock inside or outside the window to see the gate refuse or pass a request.
    /// </summary>
    protected void EnableMaintenance(string start = "22:00", string end = "23:00")
    {
        Factory.Settings.Apply(
            ("Maintenance:Enabled", "true"),
            ("Maintenance:StartTime", start),
            ("Maintenance:EndTime", end));
    }

    /// <summary>
    /// Lets <c>SeedDevelopmentDataAsync</c> seed the shared host, which is a Development host, until the test ends.
    /// </summary>
    protected void EnableSeeding()
    {
        Factory.Settings.Apply(("Database:SeedOnStartup", "true"));
    }

    protected async Task<T> CreateAsync<T>(string url, object body)
    {
        var response = await Client.PostJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadAsAsync<T>();
    }

    protected Task<BrandBody> CreateBrandAsync(string brandName = "Toyota")
    {
        return CreateAsync<BrandBody>("/api/v2/brands", new BrandRequest(brandName));
    }

    protected Task<ColorBody> CreateColorAsync(string colorName = "Red")
    {
        return CreateAsync<ColorBody>("/api/v2/colors", new ColorRequest(colorName));
    }

    protected async Task<CarResponse> CreateCarAsync(string brandName = "Toyota", string colorName = "Red", string description = "Corolla 1.6")
    {
        var brand = await CreateBrandAsync(brandName);
        var color = await CreateColorAsync(colorName);

        return await CreateCarAsync(brand.Id, color.Id, description);
    }

    protected Task<CarResponse> CreateCarAsync(int brandId, int colorId, string description = "Corolla 1.6")
    {
        return CreateAsync<CarResponse>("/api/v2/cars", new CarRequest(brandId, colorId, 2021, 950m, description, 700));
    }

    protected Task<UserResponse> CreateUserAsync(string email, string firstName = "Sezer")
    {
        return CreateAsync<UserResponse>("/api/v2/users", new UserRequest(firstName, "Tetik", email, Password));
    }

    /// <summary>Creates a customer together with the user it belongs to; the email tells the users apart.</summary>
    protected async Task<CustomerBody> CreateCustomerAsync(string email = "renter@example.com")
    {
        var user = await CreateUserAsync(email);
        return await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(user.Id));
    }

    /// <summary>Deactivates the user through the API, so every module reacts as it does for a real deactivation.</summary>
    protected async Task DeactivateUserAsync(int userId)
    {
        var response = await Client.PostJsonAsync($"/api/v2/users/{userId}/deactivation", new { });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Logs in as the user with <see cref="Password"/>, so every later request of <see cref="IntegrationTestBase.Client"/>
    /// carries its access token. The token's times come from the test clock, so do not freeze it before signing in.
    /// </summary>
    protected async Task SignInAsync(string email)
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostJsonAsync("/api/v2/auth/tokens", new VerifyCredentialsQuery(email, Password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await response.ReadAsAsync<AccessToken>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    protected async Task<UserResponse> SignInAsNewUserAsync(string email, string firstName = "Sezer")
    {
        var user = await CreateUserAsync(email, firstName);
        await SignInAsync(email);
        return user;
    }

    /// <summary>Registers a company for the signed-in user, who becomes its Owner.</summary>
    protected Task<CompanyBody> RegisterCompanyAsync(string name = "Rentar Logistics")
    {
        return CreateAsync<CompanyBody>("/api/v2/companies", new CompanyRequest(name));
    }

    protected async Task<int> RoleIdAsync(int companyId, string roleName)
    {
        var response = await Client.GetJsonAsync($"/api/v2/companies/{companyId}/roles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.ReadItemsAsync<RoleBody>()).Single(role => role.Name == roleName).Id;
    }

    /// <summary>Employs the user with one predefined role, on behalf of the signed-in user.</summary>
    protected async Task<EmployeeBody> AddEmployeeAsync(int companyId, int userId, string roleName)
    {
        var roleId = await RoleIdAsync(companyId, roleName);
        return await CreateAsync<EmployeeBody>($"/api/v2/companies/{companyId}/employees", new EmployeeRequest(userId, [roleId]));
    }

    protected async Task<HttpResponseMessage> UploadImageAsync(int carId, string fileName, byte[] bytes)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "file", fileName);

        return await Client.PostAsync($"/api/v2/cars/{carId}/images", form, TestContext.Current.CancellationToken);
    }

    protected string UploadedFile(string fileName)
    {
        var storage = (LocalImageStorage)Factory.Services.GetRequiredService<IImageStorage>();
        return Path.Combine(storage.UploadsPath, fileName);
    }

    /// <summary>
    /// Asserts an RFC 9457 problem response with the given status and, when one is given, detail.
    /// </summary>
    protected static async Task<ProblemDetails> AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string detail = null)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.ReadAsAsync<ProblemDetails>();
        Assert.Equal((int)status, problem.Status);

        if (detail != null)
        {
            Assert.Equal(detail, problem.Detail);
        }

        return problem;
    }

    protected static async Task<HttpValidationProblemDetails> AssertValidationProblemAsync(HttpResponseMessage response, params string[] fields)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.ReadAsAsync<HttpValidationProblemDetails>();
        foreach (var field in fields)
        {
            Assert.Contains(field, problem.Errors.Keys);
        }

        return problem;
    }
}
