using Rentar.Shared.Kernel.Entities;
using System;

namespace Rentar.Modules.Rentals.Domain;

public class Rental : IEntity
{
    public int Id { get; init; }

    /// <summary>Id of the Fleet car; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int CarId { get; init; }

    /// <summary>Id of the Customers customer; a reference across modules, so not a foreign key.</summary>
    public int CustomerId { get; init; }

    public DateTime RentDate { get; init; }
    public DateTime ReturnDate { get; init; }

    public RentalStatus Status { get; init; }

    /// <summary>The same rental in <paramref name="status"/>.</summary>
    public Rental WithStatus(RentalStatus status)
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
