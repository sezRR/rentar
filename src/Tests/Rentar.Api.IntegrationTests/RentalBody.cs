using Rentar.Modules.Rentals.Domain;
using System;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// A rental as the API sends and takes it. Tests use this rather than the domain's <c>Rental</c>, which can only be
/// built valid and pending, so they can send whatever a client might.
/// </summary>
public sealed class RentalBody
{
    public int Id { get; init; }
    public int CarId { get; init; }
    public int CustomerId { get; init; }
    public DateTime RentDate { get; init; }
    public DateTime ReturnDate { get; init; }
    public RentalStatus Status { get; init; }
}
