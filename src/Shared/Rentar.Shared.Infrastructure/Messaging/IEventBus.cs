using Rentar.Shared.Kernel.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Carries an event from the outbox to its subscribers. The task completes once the bus has taken responsibility for
/// it; until then the message stays in the outbox, to be published again. Delivery is at least once, so every message
/// carries its outbox id for the subscribers' inboxes to recognise it by.
/// </summary>
/// <remarks>
/// <see cref="InMemoryEventBus"/> delivers within the process. A broker (RabbitMQ, Azure Service Bus, ...) can take its
/// place, with its consumers handing what they receive to <see cref="IntegrationEventDispatcher"/>.
/// </remarks>
public interface IEventBus
{
    Task PublishAsync(Guid messageId, IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
