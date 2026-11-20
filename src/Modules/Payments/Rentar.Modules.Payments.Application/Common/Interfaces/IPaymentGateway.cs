using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application.Common.Interfaces;

/// <summary>Whoever actually takes the money. No card data is kept, so today the outcome is simulated.</summary>
public interface IPaymentGateway
{
    /// <summary>Takes a payment; answers <c>true</c> when it went through, <c>false</c> when it was declined.</summary>
    Task<bool> ChargeAsync(CancellationToken cancellationToken = default);
}
