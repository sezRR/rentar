using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Delivers the outboxes whenever an event was committed, and every <see cref="OutboxOptions.PollingInterval"/> for
/// retries and for events committed by another process.
/// </summary>
public sealed class OutboxBackgroundService(
    OutboxProcessor processor,
    OutboxSignal signal,
    IOptions<OutboxOptions> options,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.DeliverInBackground)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // A pass that published something may have led to new events, e.g. a payment's outcome; look again at once.
                while (await processor.ProcessAsync(stoppingToken) > 0)
                {
                }
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                // E.g. the database is not reachable, or not migrated yet; the next pass tries again.
                logger.LogError(exception, "Delivering the outboxes failed");
            }

            try
            {
                using var poll = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                poll.CancelAfter(options.Value.PollingInterval);
                await signal.WaitAsync(poll.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // The polling interval passed without a signal.
            }
        }
    }
}
