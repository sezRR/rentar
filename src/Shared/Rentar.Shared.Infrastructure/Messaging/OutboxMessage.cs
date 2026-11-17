using System;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// An integration event a module published, kept in that module's schema until it has been delivered to every
/// subscriber. It is stored in the same transaction as the change it tells about, so an event exists exactly when its
/// change does.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Also the message id subscribers recognise a redelivery by. Version 7, so ids sort by creation.</summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The full name of the event's type, e.g. <c>Rentar.Modules.Companies.Contracts.Events.EmployeeRemovedEvent</c>.
    /// </summary>
    public string Type { get; init; }

    /// <summary>The event as JSON.</summary>
    public string Content { get; init; }

    public DateTimeOffset OccurredOn { get; init; }

    /// <summary>When every subscriber had handled it, or <c>null</c> while it is still to be delivered.</summary>
    public DateTimeOffset? ProcessedOn { get; init; }

    /// <summary>Failed deliveries so far. Once it reaches <see cref="OutboxOptions.MaxAttempts"/> the message is left alone.</summary>
    public int Attempts { get; init; }

    /// <summary>When it is due to be delivered: at once, or after a back-off once a delivery failed.</summary>
    public DateTimeOffset NextAttemptOn { get; init; }

    /// <summary>Why the last delivery failed, or <c>null</c>.</summary>
    public string? Error { get; init; }
}
