using Rentar.Shared.Kernel.Events;

namespace Rentar.Modules.Customers.Contracts.Events;

/// <summary>A customer account was opened for the Identity user <paramref name="UserId"/>.</summary>
/// <param name="CompanyId">The Companies company the customer rents for, or <c>null</c>.</param>
public sealed record CustomerCreatedEvent(int CustomerId, int UserId, int? CompanyId) : IIntegrationEvent;
