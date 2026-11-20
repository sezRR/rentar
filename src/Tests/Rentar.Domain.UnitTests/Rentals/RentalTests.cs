using Rentar.Modules.Rentals.Domain;
using System;
using System.Linq;
using Xunit;

namespace Rentar.Domain.UnitTests.Rentals;

public class RentalTests
{
    private static readonly DateTime RentDate = new(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime ReturnDate = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Unspecified);

    private static readonly RentalCarSnapshot Car = new(
        BrandId: 1, BrandName: "Toyota", ColorId: 2, ColorName: "Blue", ModelYear: 2024, Description: "Corolla");

    private static readonly RentalCustomerSnapshot Customer = new(
        FirstName: "Ada", LastName: "Lovelace", CompanyId: 5, CompanyName: "Acme");

    private const decimal DailyPrice = 1500m;

    [Fact]
    public void Constructor_KeepsWhatItWasGiven_AndStartsPending()
    {
        var rental = new Rental(carId: 3, customerId: 4, RentDate, ReturnDate, Car, Customer, DailyPrice);

        Assert.Equal(3, rental.CarId);
        Assert.Equal(4, rental.CustomerId);
        Assert.Equal(RentDate, rental.RentDate);
        Assert.Equal(ReturnDate, rental.ReturnDate);
        Assert.Equal(RentalStatus.Pending, rental.Status);
        Assert.Equal(Car, rental.Car);
        Assert.Equal(Customer, rental.Customer);
        Assert.Equal(DailyPrice, rental.DailyPrice);
    }

    [Fact]
    public void Constructor_RefusesMissingRentDate()
    {
        Refused.With(DomainMessages.RentalRentDateMissing, () => NewRental(default, ReturnDate));
    }

    [Fact]
    public void Constructor_RefusesReturnDateOnTheRentDate()
    {
        Refused.With(DomainMessages.RentalReturnDateNotAfterRentDate, () => NewRental(RentDate, RentDate));
    }

    [Fact]
    public void Constructor_RefusesReturnDateBeforeTheRentDate()
    {
        Refused.With(DomainMessages.RentalReturnDateNotAfterRentDate, () => NewRental(ReturnDate, RentDate));
    }

    [Fact]
    public void Constructor_TakesReturnDateJustAfterTheRentDate()
    {
        var rental = NewRental(RentDate, RentDate.AddTicks(1));

        Assert.Equal(RentDate.AddTicks(1), rental.ReturnDate);
    }

    [Fact]
    public void Constructor_RefusesNegativeDailyPrice()
    {
        Refused.With(
            DomainMessages.RentalDailyPriceNegative,
            () => new Rental(3, 4, RentDate, ReturnDate, Car, Customer, dailyPrice: -0.01m));
    }

    [Fact]
    public void Constructor_TakesFreeCar()
    {
        var rental = new Rental(3, 4, RentDate, ReturnDate, Car, Customer, dailyPrice: 0m);

        Assert.Equal(0m, rental.TotalPrice);
    }

    [Fact]
    public void Constructor_RefusesMissingCar()
    {
        Assert.Throws<ArgumentNullException>(() => new Rental(3, 4, RentDate, ReturnDate, null, Customer, DailyPrice));
    }

    [Fact]
    public void Constructor_RefusesMissingCustomer()
    {
        Assert.Throws<ArgumentNullException>(() => new Rental(3, 4, RentDate, ReturnDate, Car, null, DailyPrice));
    }

    [Theory]
    [InlineData(0, 0, 1, 1)] // one tick
    [InlineData(0, 1, 0, 1)] // one hour
    [InlineData(1, 0, 0, 1)] // exactly a day
    [InlineData(1, 0, 1, 2)] // a day and a tick start a second day
    [InlineData(2, 0, 0, 2)]
    [InlineData(2, 1, 0, 3)]
    [InlineData(30, 0, 0, 30)]
    public void Constructor_BillsEveryStartedDay(int days, int hours, long ticks, int billedDays)
    {
        var returnDate = RentDate.AddDays(days).AddHours(hours).AddTicks(ticks);

        var rental = NewRental(RentDate, returnDate);

        Assert.Equal(DailyPrice * billedDays, rental.TotalPrice);
    }

    [Fact]
    public void Constructor_BillsAcrossMidnightByHoursNotDates()
    {
        // 18:00 to 09:00 the next morning is 15 hours: one day, although two dates are touched.
        var rentDate = new DateTime(2026, 9, 18, 18, 0, 0, DateTimeKind.Unspecified);

        var rental = NewRental(rentDate, rentDate.AddHours(15));

        Assert.Equal(DailyPrice, rental.TotalPrice);
    }

    [Fact]
    public void Confirmed_ConfirmsPendingRental_AndKeepsTheRest()
    {
        var pending = NewRental();

        var confirmed = pending.Confirmed();

        Assert.Equal(RentalStatus.Confirmed, confirmed.Status);
        Assert.Equal(RentalStatus.Pending, pending.Status);
        AssertSameExceptStatus(pending, confirmed);
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
        Assert.Equal(RentalStatus.Pending, pending.Status);
        AssertSameExceptStatus(pending, cancelled);
    }

    [Fact]
    public void Cancelled_CancelsConfirmedRental_AndKeepsTheRest()
    {
        // A confirmed rental that has not started is cancelled when its customer's user is deactivated.
        var confirmed = NewRental().Confirmed();

        var cancelled = confirmed.Cancelled();

        Assert.Equal(RentalStatus.Cancelled, cancelled.Status);
        AssertSameExceptStatus(confirmed, cancelled);
    }

    [Fact]
    public void Cancelled_RefusesCancelledRental()
    {
        var cancelled = NewRental().Cancelled();

        Refused.With(DomainMessages.RentalAlreadyCancelled, () => cancelled.Cancelled());
    }

    private static Rental NewRental()
    {
        return NewRental(RentDate, ReturnDate);
    }

    private static Rental NewRental(DateTime rentDate, DateTime returnDate)
    {
        return new Rental(carId: 3, customerId: 4, rentDate, returnDate, Car, Customer, DailyPrice);
    }

    /// <summary>
    /// Compares every property, so one added to <see cref="Rental"/> but not copied by a status change fails here
    /// rather than being wiped when the changed rental is stored.
    /// </summary>
    private static void AssertSameExceptStatus(Rental expected, Rental actual)
    {
        var properties = typeof(Rental).GetProperties().Where(p => p.Name != nameof(Rental.Status)).ToList();
        Assert.Contains(properties, p => p.Name == nameof(Rental.Car));

        foreach (var property in properties)
        {
            Assert.True(
                Equals(property.GetValue(expected), property.GetValue(actual)),
                $"{property.Name} was not kept: expected {property.GetValue(expected)}, got {property.GetValue(actual)}.");
        }
    }
}
