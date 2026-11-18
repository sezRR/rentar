using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
using Rentar.Modules.Identity.Application;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Presentation.Endpoints.V2;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

/// <summary>
/// Deactivating a user in Identity, and what Companies, Customers and Rentals do about it once
/// <c>UserDeactivatedEvent</c> is delivered.
/// </summary>
public class UserDeactivationTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Deactivate_MakesTheUserInactive_SoTheyCanNoLongerSignIn()
    {
        var user = await CreateUserAsync("driver@example.com");

        var response = await Client.PostJsonAsync($"/api/v2/users/{user.Id}/deactivation", new { });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False((await UserAsync(user.Id)).Status);

        var signIn = await Client.PostJsonAsync("/api/v2/auth/tokens", new VerifyCredentialsQuery("driver@example.com", Password));
        await AssertProblemAsync(signIn, HttpStatusCode.Forbidden, Messages.UserNotActive);
    }

    [Fact]
    public async Task Deactivate_AnInactiveUser_ChangesNothing()
    {
        var user = await CreateUserAsync("driver@example.com");
        await DeactivateUserAsync(user.Id);

        var response = await Client.PostJsonAsync($"/api/v2/users/{user.Id}/deactivation", new { });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False((await UserAsync(user.Id)).Status);
    }

    [Fact]
    public async Task Deactivate_AnUnknownUser_IsNotFound()
    {
        var response = await Client.PostJsonAsync("/api/v2/users/404/deactivation", new { });

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "User 404 was not found.");
    }

    [Fact]
    public async Task Deactivate_RemovesTheUserFromTheirCompanies_AndTheirCustomerFromItsCompany()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var member = await CreateUserAsync("member@example.com");
        var employee = await AddEmployeeAsync(company.Id, member.Id, "Member");
        var customer = await CreateAsync<CustomerBody>("/api/v2/customers", new CreateCustomerCommand(member.Id, company.Id));

        await DeactivateUserAsync(member.Id);
        await DeliverEventsAsync();

        Assert.DoesNotContain(employee.Id, (await EmployeesAsync(company.Id)).Select(e => e.Id));
        var fetched = await (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}")).ReadAsAsync<CustomerBody>();
        Assert.Null(fetched.CompanyId);
    }

    [Fact]
    public async Task Deactivate_TheLastEmployeeHoldingEveryPermission_IsAConflict_AndChangesNothing()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();

        var response = await Client.PostJsonAsync($"/api/v2/users/{owner.Id}/deactivation", new { });

        await AssertProblemAsync(response, HttpStatusCode.Conflict, Messages.UserIsLastFullAccessEmployee);
        Assert.True((await UserAsync(owner.Id)).Status);
        Assert.Equal([owner.Id], (await EmployeesAsync(company.Id)).Select(e => e.UserId));
    }

    [Fact]
    public async Task Deactivate_AnOwner_WhileAnotherOwnerStays_RemovesThemFromTheCompany()
    {
        var owner = await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var secondOwner = await CreateUserAsync("second-owner@example.com");
        await AddEmployeeAsync(company.Id, secondOwner.Id, "Owner");
        await SignInAsync("second-owner@example.com");

        await DeactivateUserAsync(owner.Id);
        await DeliverEventsAsync();

        Assert.Equal([secondOwner.Id], (await EmployeesAsync(company.Id)).Select(e => e.UserId));
    }

    [Fact]
    public async Task Deactivate_CancelsTheRentalsThatHaveNotStarted_AndKeepsTheOthers()
    {
        FreezeClockAtNoon();
        var today = DateTime.Today.AddHours(10);
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var past = await RentAsync(car.Id, customer.Id, today.AddDays(-10), today.AddDays(-8));
        var started = await RentAsync(car.Id, customer.Id, today.AddDays(-1), today.AddDays(2));
        var upcoming = await RentAsync(car.Id, customer.Id, today.AddDays(5), today.AddDays(7));
        var someoneElses = await RentAsync(car.Id, (await CreateCustomerAsync("other@example.com")).Id, today.AddDays(10), today.AddDays(12));
        await DeliverEventsAsync();

        await DeactivateUserAsync(customer.UserId);
        await DeliverEventsAsync();

        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(past.Id)).Status);
        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(started.Id)).Status);
        Assert.Equal(RentalStatus.Cancelled, (await RentalAsync(upcoming.Id)).Status);
        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(someoneElses.Id)).Status);
    }

    private Task<RentalBody> RentAsync(int carId, int customerId, DateTime rentDate, DateTime returnDate)
    {
        return CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(carId, customerId, rentDate, returnDate));
    }

    private async Task<RentalBody> RentalAsync(int rentalId)
    {
        return await (await Client.GetJsonAsync($"/api/v2/rentals/{rentalId}")).ReadAsAsync<RentalBody>();
    }

    private async Task<UserResponse> UserAsync(int userId)
    {
        return await (await Client.GetJsonAsync($"/api/v2/users/{userId}")).ReadAsAsync<UserResponse>();
    }

    private async Task<List<EmployeeBody>> EmployeesAsync(int companyId)
    {
        var response = await Client.GetJsonAsync($"/api/v2/companies/{companyId}/employees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadItemsAsync<EmployeeBody>();
    }
}
