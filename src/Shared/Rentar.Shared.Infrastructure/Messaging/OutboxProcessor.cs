using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Publishes the due messages of every module's outbox to the <see cref="IEventBus"/>. A message is marked processed
/// once the bus took it; a failed one waits twice as long before each retry, and after
/// <see cref="OutboxOptions.MaxAttempts"/> stays in the outbox with its error, for someone to look at.
/// </summary>
/// <remarks>
/// Messages of one module go out in the order they were published, but a failed message does not hold back the ones
/// after it. Should two processes deliver the same message, the subscribers' inboxes keep it from being handled twice.
/// </remarks>
public sealed class OutboxProcessor(
    IEnumerable<IModuleMessageStore> stores,
    IntegrationEventSerializer serializer,
    IEventBus eventBus,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger)
{
    private const int MaxErrorLength = 4000;

    private readonly IReadOnlyList<IModuleMessageStore> _stores = stores.ToList();

    /// <summary>One pass over every outbox; answers how many messages it published.</summary>
    public async Task<int> ProcessAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var published = 0;

        foreach (var store in _stores)
        {
            var due = await store.GetDueAsync(timeProvider.GetUtcNow(), settings.BatchSize, settings.MaxAttempts, cancellationToken);

            foreach (var message in due)
            {
                try
                {
                    await eventBus.PublishAsync(message.Id, serializer.Deserialize(message.Type, message.Content), cancellationToken);
                    await store.MarkProcessedAsync(message.Id, timeProvider.GetUtcNow(), cancellationToken);
                    published++;
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    await RecordFailureAsync(store, message, exception, settings, cancellationToken);
                }
            }
        }

        return published;
    }

    private async Task RecordFailureAsync(
        IModuleMessageStore store,
        OutboxMessage message,
        Exception exception,
        OutboxOptions settings,
        CancellationToken cancellationToken)
    {
        var attempts = message.Attempts + 1;
        var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempts), settings.MaxRetryDelay.TotalSeconds));
        var error = exception.ToString();

        if (attempts >= settings.MaxAttempts)
        {
            logger.LogError(exception, "Giving up on {Type} {MessageId} in {Schema} after {Attempts} attempts", message.Type, message.Id, store.Schema, attempts);
        }
        else
        {
            logger.LogWarning("Delivering {Type} {MessageId} in {Schema} failed; retrying in {Delay}", message.Type, message.Id, store.Schema, delay);
        }

        await store.RecordFailureAsync(
            message.Id,
            attempts,
            timeProvider.GetUtcNow() + delay,
            error.Length > MaxErrorLength ? error[..MaxErrorLength] : error,
            cancellationToken);
    }
}
