using Rentar.Shared.Kernel.Events;

namespace Rentar.Modules.Companies.Contracts.Events;

/// <summary>The Identity user <paramref name="UserId"/> no longer works at the company <paramref name="CompanyId"/>.</summary>
public sealed record EmployeeRemovedEvent(int CompanyId, int UserId) : IIntegrationEvent;
