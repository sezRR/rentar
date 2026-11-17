namespace Rentar.Api.IntegrationTests;

/// <param name="Event">The event type's name, e.g. <c>EmployeeRemovedEvent</c>.</param>
/// <param name="Processed">Whether every subscriber has handled it.</param>
public sealed record OutboxRow(string Event, bool Processed, int Attempts, string Error);
