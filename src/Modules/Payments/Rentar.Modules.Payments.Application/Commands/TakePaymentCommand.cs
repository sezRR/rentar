using Rentar.Shared.Application.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application.Commands;

/// <summary>Takes a payment and answers <c>true</c> when it went through, <c>false</c> when it was declined.</summary>
public sealed record TakePaymentCommand;

public sealed class TakePaymentCommandHandler : ICommandHandler<TakePaymentCommand, bool>
{
    /// <remarks>Simulated in memory, so it completes at once; a real payment provider would be awaited here.</remarks>
    public Task<bool> HandleAsync(TakePaymentCommand command, CancellationToken cancellationToken = default)
    {
        // No card data is kept, so the outcome of a payment is simulated.
        return Task.FromResult(Random.Shared.Next(2) == 1);
    }
}
