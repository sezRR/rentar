using JetBrains.Annotations;
using Rentar.Shared.Kernel.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Events;

/// <summary>
/// One module's reaction to <typeparamref name="TEvent"/>, published by another. Like an endpoint, it only adapts: it
/// turns the event into one of its own module's use cases and runs that.
/// </summary>
/// <remarks>
/// It runs after the publisher's transaction committed, once per event even when the event is delivered again; it is
/// retried when it fails, so the use case it runs should expect to find the change already made, or overtaken.
/// </remarks>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
