using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// The outbox and inbox of one module, in its own schema. Every operation opens its own context, so inside an ambient
/// transaction it shares the transaction (and, for the same module, the connection) with the module's repositories.
/// </summary>
public interface IModuleMessageStore
{
    /// <summary>The module's schema, which holds its <c>OutboxMessages</c> and <c>InboxMessages</c>.</summary>
    string Schema { get; }

    /// <summary>The module's Contracts: the events declared there are the ones it publishes, into this outbox.</summary>
    Assembly ContractsAssembly { get; }

    /// <summary>The module's Application: the event handlers declared there record what they handled in this inbox.</summary>
    Assembly ApplicationAssembly { get; }

    Task AddToOutboxAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Up to <paramref name="batchSize"/> messages not delivered yet, due by <paramref name="now"/> and with attempts
    /// left, oldest first.
    /// </summary>
    Task<List<OutboxMessage>> GetDueAsync(DateTimeOffset now, int batchSize, int maxAttempts, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid messageId, DateTimeOffset processedOn, CancellationToken cancellationToken = default);

    Task RecordFailureAsync(Guid messageId, int attempts, DateTimeOffset nextAttemptOn, string error, CancellationToken cancellationToken = default);

    Task<bool> HasHandledAsync(Guid messageId, string handler, CancellationToken cancellationToken = default);

    Task MarkHandledAsync(Guid messageId, string handler, DateTimeOffset processedOn, CancellationToken cancellationToken = default);
}
