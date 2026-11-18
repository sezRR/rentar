using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Rentar.Shared.Kernel.Paging;

namespace Rentar.Shared.Presentation.Endpoints;

/// <summary>
/// The page a v2 list is asked for in its query string, bound with <c>[AsParameters]</c>: <c>?limit=</c>, how many
/// items, at most <see cref="PageRequest.MaxSize"/>, and <c>?cursor=</c>, the <c>nextCursor</c> of the page before.
/// Leaving out the cursor asks for the first page, and leaving out the limit for <see cref="PageRequest.DefaultSize"/>
/// items. The use case refuses a limit out of range with a validation problem.
/// </summary>
public sealed class PageParameters
{
    [FromQuery(Name = "cursor")]
    public string Cursor { get; set; }

    [FromQuery(Name = "limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Throws <see cref="ValidationException"/>, which answers a validation problem naming <c>cursor</c>, for a cursor
    /// this API did not write.
    /// </summary>
    public PageRequest ToRequest()
    {
        int? after = null;
        if (!string.IsNullOrEmpty(Cursor))
        {
            if (!PageCursor.TryDecode(Cursor, out var key))
            {
                throw new ValidationException([new ValidationFailure("cursor", "'cursor' is not a cursor this API gave out.")]);
            }

            after = key;
        }

        return new PageRequest(after, Limit ?? PageRequest.DefaultSize);
    }
}
