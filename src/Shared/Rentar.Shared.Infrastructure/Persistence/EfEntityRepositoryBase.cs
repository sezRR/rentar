using Microsoft.EntityFrameworkCore;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Persistence;

/// <summary>
/// Opens a short-lived context per operation, so repositories stay safe to share as singletons. Inside an
/// ambient <see cref="System.Transactions.TransactionScope"/> Npgsql hands every context the same physical
/// connection, which keeps a multi-step operation in one local transaction.
/// </summary>
/// <remarks>
/// Entities are built once and never edited, so a write states what happens to exactly the entity it is given:
/// its navigations are left alone, and an update or delete of an entity that is not stored fails rather than
/// inserting it.
/// </remarks>
public abstract class EfEntityRepositoryBase<TEntity, TContext>(IDbContextFactory<TContext> contextFactory) : IEntityRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    protected IDbContextFactory<TContext> ContextFactory { get; } = contextFactory;

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().Where(filter).ToListAsync(cancellationToken);
    }

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().SingleOrDefaultAsync(filter, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().AnyAsync(filter, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().CountAsync(filter, cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return SaveAsync(entity, EntityState.Added, cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return SaveAsync(entity, EntityState.Modified, cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return SaveAsync(entity, EntityState.Deleted, cancellationToken);
    }

    /// <remarks>
    /// One <c>DELETE</c> statement: the database's cascades apply as they do for an entity deleted with nothing loaded.
    /// Nothing deleted fails with the exception EF throws when a delete matches no row.
    /// </remarks>
    public async Task DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Set<TEntity>().Where(filter).ExecuteDeleteAsync(cancellationToken) == 0)
        {
            throw new DbUpdateConcurrencyException($"No {typeof(TEntity).Name} matched the delete.");
        }
    }

    private async Task SaveAsync(TEntity entity, EntityState state, CancellationToken cancellationToken)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        context.Entry(entity).State = state;
        await context.SaveChangesAsync(cancellationToken);
    }
}
