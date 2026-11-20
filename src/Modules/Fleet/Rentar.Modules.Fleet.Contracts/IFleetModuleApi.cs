using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Contracts;

/// <summary>
/// What other modules may ask the Fleet module.
/// </summary>
public interface IFleetModuleApi
{
    /// <summary>Cars with the given ids, with their brand and color; unknown ids are left out.</summary>
    Task<IReadOnlyList<CarSummary>> GetCarsAsync(IReadOnlyCollection<int> carIds, CancellationToken cancellationToken = default);
}
