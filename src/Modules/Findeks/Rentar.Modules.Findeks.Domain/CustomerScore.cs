using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Findeks.Domain;

/// <summary>
/// The scores a customer was given, one row per customer. Today that is the Findeks credit rating; other scoring
/// mechanisms become further columns here.
/// </summary>
public class CustomerScore : IEntity
{
    /// <summary>
    /// Id of the Customers customer, and the key: a reference across modules, so not a foreign key.
    /// </summary>
    public int CustomerId { get; init; }

    /// <summary>Between 0 and 1900, as calculated by <c>FindeksRating.Calculate</c>.</summary>
    public int FindeksRating { get; init; }
}
