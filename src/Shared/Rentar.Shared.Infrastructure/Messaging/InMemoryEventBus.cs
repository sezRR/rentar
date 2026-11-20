using Rentar.Shared.Kernel.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Delivers within the process: the publish completes once every subscriber has handled the event, and fails when one
/// of them failed, so the message stays in the outbox until all of them have.
/// </summary>
internal sealed class InMemoryEventBus(IntegrationEventDispatcher dispatcher) : IEventBus
{
    public Task PublishAsync(Guid messageId, IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return dispatcher.DispatchAsync(messageId, integrationEvent, cancellationToken);
    }
}
