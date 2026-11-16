using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Contracts;

/// <summary>
/// What other modules may ask the Customers module.
/// </summary>
public interface ICustomersModuleApi
{
    /// <summary>Customers with the given ids; unknown ids are left out.</summary>
    Task<IReadOnlyList<CustomerSummary>> GetCustomersAsync(IReadOnlyCollection<int> customerIds, CancellationToken cancellationToken = default);
}
