using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Contracts;

/// <summary>
/// What other modules may ask the Rentals module.
/// </summary>
public interface IRentalsModuleApi
{
    /// <summary>Whether no rental of the car overlaps the given period.</summary>
    Task<bool> IsCarAvailableAsync(int carId, DateTime rentDate, DateTime returnDate, CancellationToken cancellationToken = default);
}
