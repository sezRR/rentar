using Microsoft.EntityFrameworkCore;
using Rentar.Shared.Kernel.Paging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Persistence;

public static class PagingExtensions
{
    /// <summary>
    /// Reads one page in the database by its key: only rows after <see cref="PageRequest.After"/>, in key order, and
    /// one row past the page, which only says whether more follow. No row is counted or skipped, so a page deep into
    /// the list costs what the first one does. A <c>null</c> page reads every row as one page.
    /// </summary>
    public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> query, PageKey<T> key, PageRequest page, CancellationToken cancellationToken)
    {
        if (page?.After is { } after)
        {
            query = query.Where(key.After(after));
        }

        query = query.OrderBy(key.Selector);

        if (page is null)
        {
            return new Page<T>(await query.ToListAsync(cancellationToken), null);
        }

        var rows = await query.Take(page.Size + 1).ToListAsync(cancellationToken);
        return Page.FromRows(rows, key.Read, page.Size);
    }
}
