using System;
using System.Linq.Expressions;

namespace Rentar.Shared.Infrastructure.Persistence;

/// <summary>
/// The unique, increasing <c>int</c> a list is ordered by and continued after, usually the id. Build one per list
/// and keep it, e.g. in a static field: reading the key back from an item compiles <see cref="Selector"/> once.
/// </summary>
public sealed class PageKey<T>(Expression<Func<T, int>> selector)
{
    public Expression<Func<T, int>> Selector { get; } = selector;

    public Func<T, int> Read { get; } = selector.Compile();

    /// <summary>The items whose key comes after <paramref name="after"/>; the key is sent as a query parameter.</summary>
    public Expression<Func<T, bool>> After(int after)
    {
        Expression<Func<int>> boundary = () => after;
        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(Selector.Body, boundary.Body), Selector.Parameters);
    }
}
