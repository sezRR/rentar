using Rentar.Shared.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Data;

/// <summary>
/// Reads and writes one entity type. Every operation reaches the database, so every operation is asynchronous.
/// </summary>
public interface IEntityRepository<TEntity> where TEntity : class, IEntity
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>The one entity that matches, or <c>null</c>; throws when more than one matches.</summary>
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes what matches without reading it first, e.g. by id: entities can only be built valid, so there is no
    /// stand-in with just a key to hand <see cref="DeleteAsync(TEntity,CancellationToken)"/>. Like that delete, it
    /// fails when nothing matches.
    /// </summary>
    Task DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
}
