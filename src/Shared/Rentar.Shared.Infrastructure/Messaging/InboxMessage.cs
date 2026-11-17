using System;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// That one of a module's event handlers has handled a message, kept in the subscriber's schema and stored in the same
/// transaction as the handler's changes. Delivery is at least once, so a message can arrive again; the inbox makes the
/// handler run once.
/// </summary>
public sealed class InboxMessage
{
    public Guid MessageId { get; init; }

    /// <summary>The full name of the event handler's type.</summary>
    public string Handler { get; init; }

    public DateTimeOffset ProcessedOn { get; init; }
}
