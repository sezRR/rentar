using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

internal sealed class ModuleMessageStore<TContext>(
    IDbContextFactory<TContext> contextFactory,
    Assembly contractsAssembly,
    Assembly applicationAssembly) : IModuleMessageStore where TContext : DbContext
{
    private readonly Lazy<string> _schema = new(() =>
    {
        using var context = contextFactory.CreateDbContext();
        return context.Model.GetDefaultSchema();
    });

    public string Schema => _schema.Value;

    public Assembly ContractsAssembly { get; } = contractsAssembly;

    public Assembly ApplicationAssembly { get; } = applicationAssembly;

    public async Task AddToOutboxAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<OutboxMessage>().Add(message);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetDueAsync(DateTimeOffset now, int batchSize, int maxAttempts, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.Attempts < maxAttempts && m.NextAttemptOn <= now)
            .OrderBy(m => m.OccurredOn)
            .ThenBy(m => m.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid messageId, DateTimeOffset processedOn, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Set<OutboxMessage>()
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(message => message
                .SetProperty(m => m.ProcessedOn, processedOn)
                .SetProperty(m => m.Error, (string?)null), cancellationToken);
    }

    public async Task RecordFailureAsync(
        Guid messageId,
        int attempts,
        DateTimeOffset nextAttemptOn,
        string error,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Set<OutboxMessage>()
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(message => message
                .SetProperty(m => m.Attempts, attempts)
                .SetProperty(m => m.NextAttemptOn, nextAttemptOn)
                .SetProperty(m => m.Error, error), cancellationToken);
    }

    public async Task<bool> HasHandledAsync(Guid messageId, string handler, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<InboxMessage>().AnyAsync(m => m.MessageId == messageId && m.Handler == handler, cancellationToken);
    }

    public async Task MarkHandledAsync(Guid messageId, string handler, DateTimeOffset processedOn, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<InboxMessage>().Add(new InboxMessage { MessageId = messageId, Handler = handler, ProcessedOn = processedOn });
        await context.SaveChangesAsync(cancellationToken);
    }
}
