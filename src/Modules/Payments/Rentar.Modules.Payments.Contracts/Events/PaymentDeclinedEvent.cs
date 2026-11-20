using Rentar.Shared.Kernel.Events;

namespace Rentar.Modules.Payments.Contracts.Events;

/// <summary>The payment for the Rentals rental <paramref name="RentalId"/> was declined.</summary>
public sealed record PaymentDeclinedEvent(int RentalId) : IIntegrationEvent;
