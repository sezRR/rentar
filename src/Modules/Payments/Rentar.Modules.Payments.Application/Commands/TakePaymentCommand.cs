using System;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Payments.Application.Commands;

/// <summary>Takes a payment and answers <c>true</c> when it went through, <c>false</c> when it was declined.</summary>
public sealed record TakePaymentCommand;

public sealed class TakePaymentCommandHandler : ICommandHandler<TakePaymentCommand, bool>
{
    public bool Handle(TakePaymentCommand command)
    {
        // No card data is kept, so the outcome of a payment is simulated.
        return Random.Shared.Next(2) == 1;
    }
}
