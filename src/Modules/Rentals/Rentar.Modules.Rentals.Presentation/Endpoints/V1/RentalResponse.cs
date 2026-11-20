using Rentar.Modules.Rentals.Domain;
using System;

namespace Rentar.Modules.Rentals.Presentation.Endpoints.V1;

/// <summary>A rental as v1 has always sent it: v1 knows no statuses, so it only ever sees rentals that hold their car.</summary>
public sealed record RentalResponse(int Id, int CarId, int CustomerId, DateTime RentDate, DateTime ReturnDate)
{
    public static RentalResponse From(Rental rental)
    {
        return new RentalResponse(rental.Id, rental.CarId, rental.CustomerId, rental.RentDate, rental.ReturnDate);
    }
}
