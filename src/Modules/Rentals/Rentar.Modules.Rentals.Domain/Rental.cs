using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System;

namespace Rentar.Modules.Rentals.Domain;

/// <summary>
/// A booking of a car for a period. It is built <see cref="RentalStatus.Pending"/> and moves on only through
/// <see cref="Confirmed"/> and <see cref="Cancelled"/>, which refuse a step its status does not allow with
/// <see cref="BusinessRuleException"/>.
/// </summary>
/// <remarks>
/// The car, the customer and the price are kept as they were when the rental was booked, so later changes in the
/// Fleet, Customers, Identity and Companies modules never rewrite a rental's history.
/// </remarks>
public class Rental : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a rental.</summary>
    private Rental()
    {
    }

    /// <summary>A new booking, pending until its payment's outcome confirms or cancels it.</summary>
    /// <param name="car">The car as it is now, kept as the rental's own copy.</param>
    /// <param name="customer">The customer's user and company as they are now, kept as the rental's own copy.</param>
    /// <param name="dailyPrice">The car's daily price now, which the rental keeps whatever the car costs later.</param>
    public Rental(
        int carId,
        int customerId,
        DateTime rentDate,
        DateTime returnDate,
        RentalCarSnapshot car,
        RentalCustomerSnapshot customer,
        decimal dailyPrice)
    {
        ArgumentNullException.ThrowIfNull(car);
        ArgumentNullException.ThrowIfNull(customer);

        if (rentDate == default)
        {
            throw new BusinessRuleException(DomainMessages.RentalRentDateMissing);
        }

        if (returnDate <= rentDate)
        {
            throw new BusinessRuleException(DomainMessages.RentalReturnDateNotAfterRentDate);
        }

        if (dailyPrice < 0)
        {
            throw new BusinessRuleException(DomainMessages.RentalDailyPriceNegative);
        }

        CarId = carId;
        CustomerId = customerId;
        RentDate = rentDate;
        ReturnDate = returnDate;
        Status = RentalStatus.Pending;
        Car = car;
        Customer = customer;
        DailyPrice = dailyPrice;
        TotalPrice = dailyPrice * BilledDays(rentDate, returnDate);
    }

    public int Id { get; private init; }

    /// <summary>Id of the Fleet car; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int CarId { get; private init; }

    /// <summary>Id of the Customers customer; a reference across modules, so not a foreign key.</summary>
    public int CustomerId { get; private init; }

    public DateTime RentDate { get; private init; }
    public DateTime ReturnDate { get; private init; }
    public RentalStatus Status { get; private init; }

    /// <summary>The car as it was when the rental was booked.</summary>
    public RentalCarSnapshot Car { get; private init; }

    /// <summary>The customer's user and company as they were when the rental was booked.</summary>
    public RentalCustomerSnapshot Customer { get; private init; }

    /// <summary>The car's daily price when the rental was booked.</summary>
    public decimal DailyPrice { get; private init; }

    /// <summary>
    /// <see cref="DailyPrice"/> for every day of the period, where each 24 hours that are started count as a whole day.
    /// </summary>
    public decimal TotalPrice { get; private init; }

    /// <summary>The same rental, paid for. Only a pending rental can be confirmed.</summary>
    public Rental Confirmed()
    {
        if (Status != RentalStatus.Pending)
        {
            throw new BusinessRuleException(DomainMessages.RentalNotPending);
        }

        return WithStatus(RentalStatus.Confirmed);
    }

    /// <summary>The same rental, cancelled, so it holds neither car nor customer. A cancelled rental stays as it is.</summary>
    public Rental Cancelled()
    {
        if (Status == RentalStatus.Cancelled)
        {
            throw new BusinessRuleException(DomainMessages.RentalAlreadyCancelled);
        }

        return WithStatus(RentalStatus.Cancelled);
    }

    /// <summary>The number of days billed: each 24 hours that are started count as a whole day.</summary>
    private static long BilledDays(DateTime rentDate, DateTime returnDate)
    {
        var ticks = (returnDate - rentDate).Ticks;
        return (ticks + TimeSpan.TicksPerDay - 1) / TimeSpan.TicksPerDay;
    }

    // An update stores every column of the rental it is given, so whatever is not copied here would be lost.
    private Rental WithStatus(RentalStatus status)
    {
        return new Rental
        {
            Id = Id,
            CarId = CarId,
            CustomerId = CustomerId,
            RentDate = RentDate,
            ReturnDate = ReturnDate,
            Status = status,
            Car = Car,
            Customer = Customer,
            DailyPrice = DailyPrice,
            TotalPrice = TotalPrice
        };
    }
}
