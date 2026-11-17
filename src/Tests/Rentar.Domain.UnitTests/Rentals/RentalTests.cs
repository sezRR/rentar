using Rentar.Modules.Rentals.Domain;
using System;
using Xunit;

namespace Rentar.Domain.UnitTests.Rentals;

public class RentalTests
{
    private static readonly DateTime RentDate = new(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime ReturnDate = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public void Constructor_KeepsWhatItWasGiven_AndStartsPending()
    {
        var rental = new Rental(carId: 3, customerId: 4, RentDate, ReturnDate);

        Assert.Equal(3, rental.CarId);
        Assert.Equal(4, rental.CustomerId);
        Assert.Equal(RentDate, rental.RentDate);
        Assert.Equal(ReturnDate, rental.ReturnDate);
        Assert.Equal(RentalStatus.Pending, rental.Status);
    }

    [Fact]
    public void Constructor_RefusesMissingRentDate()
    {
        Refused.With(DomainMessages.RentalRentDateMissing, () => new Rental(3, 4, default, ReturnDate));
    }

    [Fact]
    public void Constructor_RefusesReturnDateOnTheRentDate()
    {
        Refused.With(DomainMessages.RentalReturnDateNotAfterRentDate, () => new Rental(3, 4, RentDate, RentDate));
    }

    [Fact]
    public void Constructor_RefusesReturnDateBeforeTheRentDate()
    {
        Refused.With(DomainMessages.RentalReturnDateNotAfterRentDate, () => new Rental(3, 4, ReturnDate, RentDate));
    }

    [Fact]
    public void Constructor_TakesReturnDateJustAfterTheRentDate()
    {
        var rental = new Rental(3, 4, RentDate, RentDate.AddTicks(1));

        Assert.Equal(RentDate.AddTicks(1), rental.ReturnDate);
    }

    [Fact]
    public void Confirmed_ConfirmsPendingRental_AndKeepsTheRest()
    {
        var pending = NewRental();

        var confirmed = pending.Confirmed();

        Assert.Equal(RentalStatus.Confirmed, confirmed.Status);
        Assert.Equal(pending.Id, confirmed.Id);
        Assert.Equal(pending.CarId, confirmed.CarId);
        Assert.Equal(pending.CustomerId, confirmed.CustomerId);
        Assert.Equal(pending.RentDate, confirmed.RentDate);
        Assert.Equal(pending.ReturnDate, confirmed.ReturnDate);
        Assert.Equal(RentalStatus.Pending, pending.Status);
    }

    [Fact]
    public void Confirmed_RefusesConfirmedRental()
    {
        var confirmed = NewRental().Confirmed();

        Refused.With(DomainMessages.RentalNotPending, () => confirmed.Confirmed());
    }

    [Fact]
    public void Confirmed_RefusesCancelledRental()
    {
        var cancelled = NewRental().Cancelled();

        Refused.With(DomainMessages.RentalNotPending, () => cancelled.Confirmed());
    }

    [Fact]
    public void Cancelled_CancelsPendingRental_AndKeepsTheRest()
    {
        var pending = NewRental();

        var cancelled = pending.Cancelled();

        Assert.Equal(RentalStatus.Cancelled, cancelled.Status);
        Assert.Equal(pending.CarId, cancelled.CarId);
        Assert.Equal(pending.CustomerId, cancelled.CustomerId);
        Assert.Equal(pending.RentDate, cancelled.RentDate);
        Assert.Equal(pending.ReturnDate, cancelled.ReturnDate);
        Assert.Equal(RentalStatus.Pending, pending.Status);
    }

    [Fact]
    public void Cancelled_CancelsConfirmedRental()
    {
        // A confirmed rental that has not started is cancelled when its customer's user is deactivated.
        var cancelled = NewRental().Confirmed().Cancelled();

        Assert.Equal(RentalStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public void Cancelled_RefusesCancelledRental()
    {
        var cancelled = NewRental().Cancelled();

        Refused.With(DomainMessages.RentalAlreadyCancelled, () => cancelled.Cancelled());
    }

    private static Rental NewRental()
    {
        return new Rental(carId: 3, customerId: 4, RentDate, ReturnDate);
    }
}
