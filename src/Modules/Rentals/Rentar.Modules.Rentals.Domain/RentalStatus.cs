namespace Rentar.Modules.Rentals.Domain;

/// <summary>Where a rental stands. Stored by name.</summary>
public enum RentalStatus
{
    /// <summary>Booked, its payment's outcome not known yet. It holds its car meanwhile.</summary>
    Pending,

    /// <summary>Paid for.</summary>
    Confirmed,

    /// <summary>Its payment was declined, or its customer's user was deactivated before it started. It holds no car.</summary>
    Cancelled
}
