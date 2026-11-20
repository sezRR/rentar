using Npgsql;
using Rentar.Modules.Rentals.Application;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class RentalBookingConflictsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    private static readonly DateTime Start = new(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime End = Start.AddDays(2);

    /// <summary>
    /// Periods overlapping <see cref="Start"/> to <see cref="End"/>, including the ones that only share its start or its
    /// end instant. The status of the rental holding the car or customer does not change this arithmetic, so the periods
    /// run against a pending rental, and one confirmed case per side covers the status.
    /// </summary>
    public static TheoryData<double, double> OverlappingPeriods()
    {
        return new TheoryData<double, double> { { 0, 2 }, { -1, 1 }, { 1, 3 }, { -1, 3 }, { 0.5, 1.5 }, { -1, 0 }, { 2, 3 } };
    }

    [Theory]
    [MemberData(nameof(OverlappingPeriods))]
    public Task Create_ForAnOccupiedCar_IsAConflict_EvenForAnotherCustomer(double from, double to)
    {
        return AssertCarConflictAsync(from, to, confirmed: false);
    }

    [Fact]
    public Task Create_ForACarAConfirmedRentalHolds_IsAConflict_EvenForAnotherCustomer()
    {
        return AssertCarConflictAsync(0, 2, confirmed: true);
    }

    [Theory]
    [MemberData(nameof(OverlappingPeriods))]
    public Task Create_ForAnOccupiedCustomer_IsAConflict_EvenForAnotherCar(double from, double to)
    {
        return AssertCustomerConflictAsync(from, to, confirmed: false);
    }

    [Fact]
    public Task Create_ForACustomerAConfirmedRentalHolds_IsAConflict_EvenForAnotherCar()
    {
        return AssertCustomerConflictAsync(0, 2, confirmed: true);
    }

    [Theory]
    [InlineData(-2, -1)]
    [InlineData(3, 4)]
    public async Task Create_ForTheSameCarAndCustomer_InANonOverlappingPeriod_IsAllowed(double from, double to)
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        await BookAsync(car.Id, customer.Id);

        var response = await Client.PostJsonAsync("/api/v2/rentals",
            new CreateRentalCommand(car.Id, customer.Id, Start.AddDays(from), Start.AddDays(to)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(2, (await RentalsAsync()).Count);
    }

    [Fact]
    public async Task Create_ForAnotherCarAndCustomer_InTheSamePeriod_IsAllowed()
    {
        var first = await CreateCarAsync();
        var second = await CreateCarAsync();
        var firstCustomer = await CreateCustomerAsync("first@example.com");
        var secondCustomer = await CreateCustomerAsync("second@example.com");
        await BookAsync(first.Id, firstCustomer.Id);

        await BookAsync(second.Id, secondCustomer.Id);

        Assert.Equal(2, (await RentalsAsync()).Count);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Create_AfterCancellation_ReleasesBothTheCarAndTheCustomer(bool sameCar, bool sameCustomer)
    {
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        await BookAsync(car.Id, customer.Id);
        Factory.PaymentGateway.Declines = true;
        await DeliverEventsAsync();
        Factory.PaymentGateway.Declines = false;
        Assert.Equal(RentalStatus.Cancelled, Assert.Single(await RentalsAsync()).Status);
        var nextCar = sameCar ? car : await CreateCarAsync();
        var nextCustomer = sameCustomer ? customer : await CreateCustomerAsync("other@example.com");

        await BookAsync(nextCar.Id, nextCustomer.Id);

        var rentals = await RentalsAsync();
        Assert.Equal(2, rentals.Count);
        Assert.Single(rentals, rental => rental.Status == RentalStatus.Pending);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task V1_AlsoRefusesOverlappingBookings_WithItsLegacyErrorEnvelope(bool sameCar)
    {
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        await BookAsync(car.Id, customer.Id);
        var nextCar = sameCar ? car : await CreateCarAsync();
        var nextCustomer = sameCar ? await CreateCustomerAsync("other@example.com") : customer;

        var response = await Client.PostJsonAsync("/api/v1/rentals/add",
            new CreateRentalCommand(nextCar.Id, nextCustomer.Id, Start, End));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.ReadAsAsync<V1Response>();
        Assert.False(body.Success);
        Assert.Equal(sameCar ? Messages.CarNotAvailable : Messages.CustomerNotAvailable, body.Message);
        Assert.Single(await RentalsAsync());
        Assert.Single(await Factory.OutboxAsync("rentals"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConcurrentBookings_ReserveTheCarOrCustomerOnlyOnce(bool sameCar)
    {
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var commands = new List<CreateRentalCommand>();
        for (var index = 0; index < 6; index++)
        {
            var nextCar = sameCar || index == 0 ? car : await CreateCarAsync();
            var nextCustomer = !sameCar || index == 0 ? customer : await CreateCustomerAsync($"renter{index}@example.com");
            commands.Add(new CreateRentalCommand(nextCar.Id, nextCustomer.Id, Start, End));
        }

        var responses = await BookConcurrentlyAsync(commands);

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        var conflicts = responses.Where(response => response.StatusCode != HttpStatusCode.Created).ToList();
        Assert.Equal(commands.Count - 1, conflicts.Count);
        foreach (var response in conflicts)
        {
            await AssertProblemAsync(response, HttpStatusCode.Conflict,
                sameCar ? Messages.CarNotAvailable : Messages.CustomerNotAvailable);
        }

        Assert.Single(await RentalsAsync());
        Assert.Single(await Factory.OutboxAsync("rentals"));
        await DeliverEventsAsync();
        Assert.Equal(1, Factory.PaymentGateway.Charges);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConcurrentNonConflictingBookings_AllSucceed(bool separatePeriods)
    {
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        var commands = new List<CreateRentalCommand>();
        for (var index = 0; index < 6; index++)
        {
            var nextCar = separatePeriods || index == 0 ? car : await CreateCarAsync();
            var nextCustomer = separatePeriods || index == 0 ? customer : await CreateCustomerAsync($"renter{index}@example.com");
            var offset = separatePeriods ? index * 3 : 0;
            commands.Add(new CreateRentalCommand(nextCar.Id, nextCustomer.Id, Start.AddDays(offset), End.AddDays(offset)));
        }

        var responses = await BookConcurrentlyAsync(commands);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(commands.Count, (await RentalsAsync()).Count);
        Assert.Equal(commands.Count, (await Factory.OutboxAsync("rentals")).Count);
        await DeliverEventsAsync();
        Assert.Equal(commands.Count, Factory.PaymentGateway.Charges);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TheDatabase_AlsoRefusesConcurrentOverlaps_WithoutAnApplicationPrecheck(bool sameCar)
    {
        await using var first = new NpgsqlConnection(Factory.ConnectionString);
        await using var second = new NpgsqlConnection(Factory.ConnectionString);
        await first.OpenAsync(TestContext.Current.CancellationToken);
        await second.OpenAsync(TestContext.Current.CancellationToken);
        await using var transaction = await first.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await using var initial = Insert(first, 1, 1);
        await initial.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        await using var competing = Insert(second, sameCar ? 1 : 2, sameCar ? 2 : 1);

        var insert = competing.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        await transaction.CommitAsync(TestContext.Current.CancellationToken);
        var exception = await Assert.ThrowsAsync<PostgresException>(async () => await insert);

        Assert.Equal(PostgresErrorCodes.ExclusionViolation, exception.SqlState);
        Assert.Equal(sameCar ? "EX_Rentals_CarPeriod" : "EX_Rentals_CustomerPeriod", exception.ConstraintName);
    }

    private static NpgsqlCommand Insert(NpgsqlConnection connection, int carId, int customerId)
    {
        return RentarApiFactory.InsertRental(connection, carId, customerId, Start, End, "Pending");
    }

    private Task<RentalBody> BookAsync(int carId, int customerId)
    {
        return CreateAsync<RentalBody>("/api/v2/rentals", new CreateRentalCommand(carId, customerId, Start, End));
    }

    private async Task<HttpResponseMessage[]> BookConcurrentlyAsync(IEnumerable<CreateRentalCommand> commands)
    {
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = commands.Select(async command =>
        {
            await start.Task.WaitAsync(TestContext.Current.CancellationToken);
            return await Client.PostJsonAsync("/api/v2/rentals", command);
        }).ToList();
        start.SetResult();
        return await Task.WhenAll(requests);
    }

    private async Task<List<RentalBody>> RentalsAsync()
    {
        return await (await Client.GetJsonAsync("/api/v2/rentals")).ReadItemsAsync<RentalBody>();
    }

    /// <summary>Books the car for one customer, then asks for it for another customer over the given period.</summary>
    private async Task AssertCarConflictAsync(double from, double to, bool confirmed)
    {
        FreezeClockAtNoon();
        var car = await CreateCarAsync();
        var first = await CreateCustomerAsync("first@example.com");
        var second = await CreateCustomerAsync("second@example.com");
        await BookAsync(car.Id, first.Id);
        if (confirmed)
        {
            await DeliverEventsAsync();
        }

        var response = await Client.PostJsonAsync("/api/v2/rentals",
            new CreateRentalCommand(car.Id, second.Id, Start.AddDays(from), Start.AddDays(to)));

        await AssertProblemAsync(response, HttpStatusCode.Conflict, Messages.CarNotAvailable);
        var stored = Assert.Single(await RentalsAsync());
        Assert.Equal(first.Id, stored.CustomerId);
        Assert.Single(await Factory.OutboxAsync("rentals"));
        await DeliverEventsAsync();
        Assert.Equal(1, Factory.PaymentGateway.Charges);
    }

    /// <summary>Books one car for the customer, then asks for another car for them over the given period.</summary>
    private async Task AssertCustomerConflictAsync(double from, double to, bool confirmed)
    {
        FreezeClockAtNoon();
        var first = await CreateCarAsync();
        var second = await CreateCarAsync();
        var customer = await CreateCustomerAsync();
        await BookAsync(first.Id, customer.Id);
        if (confirmed)
        {
            await DeliverEventsAsync();
        }

        var response = await Client.PostJsonAsync("/api/v2/rentals",
            new CreateRentalCommand(second.Id, customer.Id, Start.AddDays(from), Start.AddDays(to)));

        await AssertProblemAsync(response, HttpStatusCode.Conflict, Messages.CustomerNotAvailable);
        var stored = Assert.Single(await RentalsAsync());
        Assert.Equal(first.Id, stored.CarId);
        Assert.Single(await Factory.OutboxAsync("rentals"));
        await DeliverEventsAsync();
        Assert.Equal(1, Factory.PaymentGateway.Charges);
    }
}
