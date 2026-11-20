using Rentar.Shared.Kernel.Paging;

namespace Rentar.Shared.Application.Data;

/// <summary>
/// A query that lists, and answers one <see cref="Page{T}"/> of its list, ordered by key. A <c>null</c>
/// <see cref="Page"/> asks for the whole list as one page: v1 never pages, and neither do use cases that need every
/// item.
/// </summary>
/// <remarks><c>PagedQueryValidator</c> checks the page of every such query, so a query does not validate it itself.</remarks>
public interface IPagedQuery
{
    PageRequest Page { get; }
}
