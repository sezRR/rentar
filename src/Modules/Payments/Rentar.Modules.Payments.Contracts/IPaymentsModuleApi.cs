using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Contracts;

/// <summary>
/// What other modules may ask the Payments module.
/// </summary>
public interface IPaymentsModuleApi
{
    /// <summary>Takes a payment; answers whether it went through.</summary>
    Task<bool> PayAsync(CancellationToken cancellationToken = default);
}
