using Rentar.Shared.Kernel.Events;

namespace Rentar.Modules.Payments.Contracts.Events;

/// <summary>The payment for the Rentals rental <paramref name="RentalId"/> went through.</summary>
public sealed record PaymentSucceededEvent(int RentalId) : IIntegrationEvent;
