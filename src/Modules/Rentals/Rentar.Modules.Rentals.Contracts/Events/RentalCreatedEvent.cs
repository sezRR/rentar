using Rentar.Shared.Kernel.Events;
using System;

namespace Rentar.Modules.Rentals.Contracts.Events;

/// <summary>
/// The Customers customer <paramref name="CustomerId"/> booked the Fleet car <paramref name="CarId"/>. The rental waits
/// for its payment: Rentals confirms it on <c>PaymentSucceededEvent</c> and cancels it on <c>PaymentDeclinedEvent</c>.
/// </summary>
public sealed record RentalCreatedEvent(int RentalId, int CarId, int CustomerId, DateTime RentDate, DateTime ReturnDate) : IIntegrationEvent;
