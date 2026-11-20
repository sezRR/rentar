using Rentar.Shared.Kernel.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Events;

/// <summary>
/// Tells every module subscribed to an event that it happened. The publisher does not know who reacts, so a module can
/// react to another without that module depending on it.
/// </summary>
/// <remarks>
/// Publishing stores the event in the publishing module's outbox, inside the use case's transaction, so the event is
/// kept exactly when the use case's changes are. It is delivered to the subscribers once that transaction has
/// committed, at least once and each in a transaction of its own: a reaction happens shortly after the use case, not
/// during it, and a reaction that fails is retried without undoing the use case. An event can only be published
/// inside a transaction, i.e. from a handler with <c>[TransactionScopeAspect]</c>.
/// </remarks>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
