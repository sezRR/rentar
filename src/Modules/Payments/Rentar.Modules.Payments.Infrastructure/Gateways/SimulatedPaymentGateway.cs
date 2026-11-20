using Rentar.Modules.Payments.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Infrastructure.Gateways;

/// <summary>Stands in for a payment provider: about half of all payments go through.</summary>
/// <remarks>Completes at once; a real provider would be awaited here.</remarks>
internal sealed class SimulatedPaymentGateway : IPaymentGateway
{
    public Task<bool> ChargeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Random.Shared.Next(2) == 1);
    }
}
