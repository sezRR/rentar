using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Findeks.Domain;

/// <summary>
/// The scores a customer was given, one row per customer. Today that is the Findeks credit rating; other scoring
/// mechanisms become further columns here. A rating out of range is refused with <see cref="BusinessRuleException"/>.
/// </summary>
public class CustomerScore : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a score.</summary>
    private CustomerScore()
    {
    }

    public CustomerScore(int customerId, int findeksRating)
    {
        if (findeksRating is < 0 or > FindeksReport.MaxRating)
        {
            throw new BusinessRuleException(DomainMessages.FindeksRatingOutOfRange);
        }

        CustomerId = customerId;
        FindeksRating = findeksRating;
    }

    /// <summary>
    /// Id of the Customers customer, and the key: a reference across modules, so not a foreign key.
    /// </summary>
    public int CustomerId { get; private init; }

    /// <summary>Between 0 and <see cref="FindeksReport.MaxRating"/>, as a <see cref="FindeksReport"/> adds up to.</summary>
    public int FindeksRating { get; private init; }
}
