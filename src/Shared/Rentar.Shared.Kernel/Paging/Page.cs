using System;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Shared.Kernel.Paging;

/// <summary>
/// One page of a list: its <see cref="Items"/>, and the key of its last item when more items follow, which the
/// next <see cref="PageRequest.After"/> continues from.
/// </summary>
public sealed record Page<T>(List<T> Items, int? NextAfter)
{
    public bool HasMore => NextAfter is not null;

    /// <summary>The same page with each item turned into what <paramref name="map"/> makes of it.</summary>
    public Page<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return new Page<TResult>(Items.ConvertAll(item => map(item)), NextAfter);
    }
}

public static class Page
{
    /// <summary>
    /// The requested page of a list that is already in memory, ordered by <paramref name="key"/>. Without a request
    /// the whole list is the one page, which is what callers that never page (v1, other use cases) ask for.
    /// </summary>
    public static Page<T> Of<T>(IEnumerable<T> items, Func<T, int> key, PageRequest request)
    {
        var ordered = items.OrderBy(key);
        if (request is null)
        {
            return new Page<T>([.. ordered], null);
        }

        var rest = request.After is { } after ? ordered.Where(item => key(item) > after) : ordered;
        return FromRows([.. rest.Take(request.Size + 1)], key, request.Size);
    }

    /// <summary>
    /// The page made of rows read one past its <paramref name="size"/>: the extra row only says that more follow,
    /// and is dropped.
    /// </summary>
    public static Page<T> FromRows<T>(List<T> rows, Func<T, int> key, int size)
    {
        if (rows.Count <= size)
        {
            return new Page<T>(rows, null);
        }

        rows.RemoveRange(size, rows.Count - size);
        return new Page<T>(rows, key(rows[^1]));
    }
}
