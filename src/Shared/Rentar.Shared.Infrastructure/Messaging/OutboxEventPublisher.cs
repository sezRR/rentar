using Rentar.Shared.Application.Events;
using Rentar.Shared.Kernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Publishes an event by adding it to the outbox of the module that declares it, inside the publishing use case's
/// transaction. Nothing is delivered yet: <see cref="OutboxProcessor"/> hands it to the <see cref="IEventBus"/> once the
/// transaction has committed, so a subscriber never sees an event whose change was rolled back, and an event whose
/// change committed is never lost.
/// </summary>
internal sealed class OutboxEventPublisher(
    IEnumerable<IModuleMessageStore> stores,
    IntegrationEventSerializer serializer,
    OutboxSignal signal,
    TimeProvider timeProvider) : IEventPublisher
{
    private readonly IReadOnlyList<IModuleMessageStore> _stores = stores.ToList();

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        // Outside a transaction the event could be stored while the change it tells about is not, or the other way round.
        var transaction = Transaction.Current
            ?? throw new InvalidOperationException(
                $"{typeof(TEvent).Name} was published outside a transaction; give the publishing handler [TransactionScopeAspect].");

        var eventType = integrationEvent.GetType();
        var store = _stores.SingleOrDefault(s => s.ContractsAssembly == eventType.Assembly)
            ?? throw new InvalidOperationException($"{eventType.FullName} is not declared by a module with messaging.");

        var (type, content) = serializer.Serialize(integrationEvent);
        var now = timeProvider.GetUtcNow();

        await store.AddToOutboxAsync(new OutboxMessage
        {
            Id = Guid.CreateVersion7(now),
            Type = type,
            Content = content,
            OccurredOn = now,
            NextAttemptOn = now
        }, cancellationToken);

        // Delivered as soon as it can be, rather than on the next poll.
        transaction.TransactionCompleted += (_, completed) =>
        {
            if (completed.Transaction?.TransactionInformation.Status == TransactionStatus.Committed)
            {
                signal.Notify();
            }
        };
    }
}
