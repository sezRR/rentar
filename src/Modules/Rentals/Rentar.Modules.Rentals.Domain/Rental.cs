using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System;

namespace Rentar.Modules.Rentals.Domain;

/// <summary>
/// A booking of a car for a period. It is built <see cref="RentalStatus.Pending"/> and moves on only through
/// <see cref="Confirmed"/> and <see cref="Cancelled"/>, which refuse a step its status does not allow with
/// <see cref="BusinessRuleException"/>.
/// </summary>
public class Rental : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a rental.</summary>
    private Rental()
    {
    }

    /// <summary>A new booking, pending until its payment's outcome confirms or cancels it.</summary>
    public Rental(int carId, int customerId, DateTime rentDate, DateTime returnDate)
    {
        if (rentDate == default)
        {
            throw new BusinessRuleException(DomainMessages.RentalRentDateMissing);
        }

        if (returnDate <= rentDate)
        {
            throw new BusinessRuleException(DomainMessages.RentalReturnDateNotAfterRentDate);
        }

        CarId = carId;
        CustomerId = customerId;
        RentDate = rentDate;
        ReturnDate = returnDate;
        Status = RentalStatus.Pending;
    }

    public int Id { get; private init; }

    /// <summary>Id of the Fleet car; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int CarId { get; private init; }

    /// <summary>Id of the Customers customer; a reference across modules, so not a foreign key.</summary>
    public int CustomerId { get; private init; }

    public DateTime RentDate { get; private init; }
    public DateTime ReturnDate { get; private init; }
    public RentalStatus Status { get; private init; }

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

    private Rental WithStatus(RentalStatus status)
    {
        return new Rental
        {
            Id = Id,
            CarId = CarId,
            CustomerId = CustomerId,
            RentDate = RentDate,
            ReturnDate = ReturnDate,
            Status = status
        };
    }
}
