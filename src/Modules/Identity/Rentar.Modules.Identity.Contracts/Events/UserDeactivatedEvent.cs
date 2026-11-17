using Rentar.Shared.Kernel.Events;

namespace Rentar.Modules.Identity.Contracts.Events;

/// <summary>The user <paramref name="UserId"/> was active and no longer is: they can not sign in any more.</summary>
public sealed record UserDeactivatedEvent(int UserId) : IIntegrationEvent;
