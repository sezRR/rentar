using Rentar.Shared.Kernel.Paging;
using System.Collections.Generic;

namespace Rentar.Shared.Presentation.Endpoints;

/// <summary>
/// The body of every v2 list: one page of <see cref="Items"/>, and under <see cref="Page"/> how to ask for the next.
/// </summary>
public sealed record PagedResponse<T>(List<T> Items, PageInfo Page)
{
    public static PagedResponse<T> From(Page<T> page)
    {
        var nextCursor = page.NextAfter is { } after ? PageCursor.Encode(after) : null;
        return new PagedResponse<T>(page.Items, new PageInfo(page.HasMore, nextCursor));
    }
}

/// <summary>
/// Whether more items follow the page, and the cursor that asks for them: pass it as <c>?cursor=</c>. On the last
/// page <see cref="HasMore"/> is <c>false</c> and <see cref="NextCursor"/> is <c>null</c>.
/// </summary>
public sealed record PageInfo(bool HasMore, string NextCursor);
