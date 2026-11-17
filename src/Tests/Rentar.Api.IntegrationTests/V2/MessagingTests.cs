using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Domain;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

/// <summary>
/// The guarantees the outbox, the event bus and the inboxes give, seen through the modules' real events.
/// </summary>
public class MessagingTests(RentarApiFactory factory) : V2TestBase(factory)
{
    private static readonly DateTime RentDate = DateTime.Today.AddDays(5).AddHours(10);

    [Fact]
    public async Task AnEvent_IsStoredInTheSameTransactionAsTheChangeItTellsAbout()
    {
        FreezeClockAtNoon();
        var user = await CreateUserAsync("renter@example.com");

        HttpResponseMessage response;
        await using (await Factory.RefuseAsync("customers", "OutboxMessages", "INSERT"))
        {
            response = await Client.PostJsonAsync("/api/v2/customers", new CreateCustomerCommand(user.Id));
        }

        await AssertProblemAsync(response, HttpStatusCode.InternalServerError);
        Assert.Empty(await (await Client.GetJsonAsync("/api/v2/customers")).ReadAsAsync<List<Customer>>());
    }

    [Fact]
    public async Task AnEventDeliveredAgain_IsHandledOnce()
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        await CreateAsync<Rental>("/api/v2/rentals", new CreateRentalCommand(car.Id, customer.Id, RentDate, RentDate.AddDays(2)));
        await DeliverEventsAsync();

        await Factory.MarkOutboxUndeliveredAsync("rentals");
        await DeliverEventsAsync();

        Assert.Equal(1, Factory.PaymentGateway.Charges);
        Assert.All(await Factory.OutboxAsync("rentals"), message => Assert.True(message.Processed));
    }

    [Fact]
    public async Task AReactionThatFails_DoesNotHoldBackTheOtherSubscribers_AndIsRetried()
    {
        await SignInAsNewUserAsync("owner@example.com");
        var company = await RegisterCompanyAsync();
        var customer = await CreateCustomerAsync("member@example.com");
        await AddEmployeeAsync(company.Id, customer.UserId, "Member");
        var rental = await CreateAsync<Rental>("/api/v2/rentals", new CreateRentalCommand((await CreateCarAsync()).Id, customer.Id, RentDate, RentDate.AddDays(2)));
        await DeliverEventsAsync();

        // Both Companies and Rentals react to the deactivation; only Rentals can not store its change.
        await using (await Factory.RefuseAsync("rentals", "Rentals", "UPDATE"))
        {
            await DeactivateUserAsync(customer.UserId);
            await DeliverEventsAsync();
        }

        Assert.DoesNotContain(customer.UserId, (await EmployeesAsync(company.Id)).Select(e => e.UserId));
        Assert.Equal(RentalStatus.Confirmed, (await RentalAsync(rental.Id)).Status);
        var deactivation = Assert.Single(await Factory.OutboxAsync("identity"));
        Assert.Equal(("UserDeactivatedEvent", false, 1), (deactivation.Event, deactivation.Processed, deactivation.Attempts));

        Factory.Clock.FreezeAt(DateTimeOffset.UtcNow.AddMinutes(1));
        await DeliverEventsAsync();

        Assert.Equal(RentalStatus.Cancelled, (await RentalAsync(rental.Id)).Status);
        Assert.True(Assert.Single(await Factory.OutboxAsync("identity")).Processed);
    }

    [Fact]
    public async Task AReactionThatKeepsFailing_IsGivenUpAfterItsAttempts_AndLeftInTheOutbox()
    {
        var start = DateTimeOffset.UtcNow;
        Customer customer;

        await using (await Factory.RefuseAsync("findeks", "CustomerScores", "INSERT"))
        {
            customer = await CreateCustomerAsync();

            // Each round is past the longest back-off, so every one of them is a delivery attempt.
            for (var round = 1; round <= 10; round++)
            {
                Factory.Clock.FreezeAt(start.AddMinutes(10 * round));
                await DeliverEventsAsync();
            }
        }

        Factory.Clock.FreezeAt(start.AddHours(5));
        await DeliverEventsAsync();

        var message = Assert.Single(await Factory.OutboxAsync("customers"));
        Assert.Equal(("CustomerCreatedEvent", false, 10), (message.Event, message.Processed, message.Attempts));
        Assert.Contains("refused by test", message.Error);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).StatusCode);
    }

    [Fact]
    public async Task TheBackgroundService_DeliversAnEventOnceItsTransactionCommits()
    {
        Assert.Contains(Factory.Services.GetServices<IHostedService>(), service => service is OutboxBackgroundService);

        // The host's own instance is off for the tests. This one runs as outside them, but polls so rarely that only the
        // commit's signal can deliver the event in time.
        var background = ActivatorUtilities.CreateInstance<OutboxBackgroundService>(
            Factory.Services,
            Options.Create(new OutboxOptions { PollingInterval = TimeSpan.FromMinutes(5) }));
        await background.StartAsync(TestContext.Current.CancellationToken);

        try
        {
            var customer = await CreateCustomerAsync();

            var deadline = DateTime.UtcNow.AddSeconds(10);
            HttpStatusCode status;
            do
            {
                status = (await Client.GetJsonAsync($"/api/v2/customers/{customer.Id}/score")).StatusCode;
                if (status != HttpStatusCode.OK)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
                }
            }
            while (status != HttpStatusCode.OK && DateTime.UtcNow < deadline);

            Assert.Equal(HttpStatusCode.OK, status);
        }
        finally
        {
            await background.StopAsync(CancellationToken.None);
        }
    }

    private async Task<Rental> RentalAsync(int rentalId)
    {
        return await (await Client.GetJsonAsync($"/api/v2/rentals/{rentalId}")).ReadAsAsync<Rental>();
    }

    private async Task<List<EmployeeResponse>> EmployeesAsync(int companyId)
    {
        var response = await Client.GetJsonAsync($"/api/v2/companies/{companyId}/employees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsAsync<List<EmployeeResponse>>();
    }
}
