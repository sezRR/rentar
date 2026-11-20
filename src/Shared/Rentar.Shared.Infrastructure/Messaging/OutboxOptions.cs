using System;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>How the outbox is delivered, bound from <c>Messaging:Outbox</c>.</summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Messaging:Outbox";

    /// <summary>
    /// Whether a background service delivers the outboxes. Tests turn it off and deliver themselves, so they know when
    /// the reactions have run.
    /// </summary>
    public bool DeliverInBackground { get; set; } = true;

    /// <summary>
    /// How often the background service looks for due messages. It also looks as soon as a transaction that published
    /// an event commits, so this mostly paces retries.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>How many due messages of one module a pass delivers.</summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Deliveries a message gets before it is left in the outbox as failed. The waits between them double from two
    /// seconds, up to <see cref="MaxRetryDelay"/>.
    /// </summary>
    public int MaxAttempts { get; set; } = 10;

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(5);
}
