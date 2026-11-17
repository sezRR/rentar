using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Kernel.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Runs every module's handler of an event once per message. Each handler runs in a transaction of its own together
/// with the inbox entry that records it, so a handler that fails leaves nothing behind and is tried again with the
/// next delivery, while the ones that succeeded are skipped then.
/// </summary>
public sealed class IntegrationEventDispatcher(
    IServiceProvider serviceProvider,
    IEnumerable<IModuleMessageStore> stores,
    TimeProvider timeProvider,
    ILogger<IntegrationEventDispatcher> logger)
{
    private static readonly MethodInfo HandleMethod =
        typeof(IntegrationEventDispatcher).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly ConcurrentDictionary<Type, Func<object, IIntegrationEvent, CancellationToken, Task>> Handlers = new();

    private readonly IReadOnlyList<IModuleMessageStore> _stores = stores.ToList();

    /// <summary>Throws, once every handler has had its turn, when any of them failed.</summary>
    public async Task DispatchAsync(Guid messageId, IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var eventType = integrationEvent.GetType();
        var handle = Handlers.GetOrAdd(eventType, type => HandleMethod.MakeGenericMethod(type)
            .CreateDelegate<Func<object, IIntegrationEvent, CancellationToken, Task>>());
        var failures = new List<Exception>();

        foreach (var handler in serviceProvider.GetServices(typeof(IIntegrationEventHandler<>).MakeGenericType(eventType)))
        {
            var handlerName = handler!.GetType().FullName!;
            var inbox = _stores.SingleOrDefault(store => store.ApplicationAssembly == handler.GetType().Assembly)
                ?? throw new InvalidOperationException($"{handlerName} belongs to no module with messaging.");

            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (!await inbox.HasHandledAsync(messageId, handlerName, cancellationToken))
                {
                    await handle(handler, integrationEvent, cancellationToken);
                    await inbox.MarkHandledAsync(messageId, handlerName, timeProvider.GetUtcNow(), cancellationToken);
                }

                scope.Complete();
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "{Handler} failed to handle {Event} {MessageId}", handlerName, eventType.Name, messageId);
                failures.Add(exception);
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException($"{failures.Count} handler(s) of {eventType.Name} {messageId} failed.", failures);
        }
    }

    private static Task HandleAsync<TEvent>(object handler, IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        return ((IIntegrationEventHandler<TEvent>)handler).HandleAsync((TEvent)integrationEvent, cancellationToken);
    }
}
