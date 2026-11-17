using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Domain;
using Rentar.Modules.Companies.Presentation.Endpoints.V2;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
using Rentar.Modules.Fleet.Domain;
using Rentar.Modules.Fleet.Infrastructure.Storage;
using Rentar.Modules.Fleet.Presentation.Endpoints.V2;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Presentation.Endpoints.V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

    protected static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
    protected static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
    protected static readonly byte[] TextBytes = Encoding.UTF8.GetBytes("# not an image");

    /// <summary>
    /// Some lists are closed during the 22:00 maintenance hour, so tests that read them pin the clock to midday.
    /// </summary>
    protected void FreezeClockAtNoon()
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(12)));
    }

    protected void FreezeClockInMaintenanceHour()
    {
        Factory.Clock.FreezeAt(new DateTimeOffset(DateTime.Today.AddHours(22).AddMinutes(30)));
    }

    protected async Task<T> CreateAsync<T>(string url, object body)
    {
        var response = await Client.PostJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadAsAsync<T>();
    }

    protected Task<Brand> CreateBrandAsync(string brandName = "Toyota")
    {
        return CreateAsync<Brand>("/api/v2/brands", new BrandRequest(brandName));
    }

    protected Task<Color> CreateColorAsync(string colorName = "Red")
    {
        return CreateAsync<Color>("/api/v2/colors", new ColorRequest(colorName));
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
    protected async Task<Customer> CreateCustomerAsync(string email = "renter@example.com")
    {
        var user = await CreateUserAsync(email);
        return await CreateAsync<Customer>("/api/v2/customers", new CreateCustomerCommand(user.Id));
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
    protected Task<Company> RegisterCompanyAsync(string name = "Rentar Logistics")
    {
        return CreateAsync<Company>("/api/v2/companies", new CompanyRequest(name));
    }

    protected async Task<int> RoleIdAsync(int companyId, string roleName)
    {
        var response = await Client.GetJsonAsync($"/api/v2/companies/{companyId}/roles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.ReadAsAsync<List<Role>>()).Single(role => role.Name == roleName).Id;
    }

    /// <summary>Employs the user with one predefined role, on behalf of the signed-in user.</summary>
    protected async Task<EmployeeResponse> AddEmployeeAsync(int companyId, int userId, string roleName)
    {
        var roleId = await RoleIdAsync(companyId, roleName);
        return await CreateAsync<EmployeeResponse>($"/api/v2/companies/{companyId}/employees", new EmployeeRequest(userId, [roleId]));
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
