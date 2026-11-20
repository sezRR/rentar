namespace Rentar.Shared.Kernel.Events;

/// <summary>
/// Something that happened in one module that other modules may react to, e.g. <c>EmployeeRemovedEvent</c>. An event
/// is a sealed record in the publishing module's Contracts, under <c>Events</c>, named in the past tense with an
/// <c>Event</c> suffix, and carries ids and facts only: a subscriber that needs more asks the publisher's module API.
/// </summary>
public interface IIntegrationEvent;
