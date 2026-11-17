using Rentar.Modules.Payments.Application.Common.Interfaces;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application.Commands;

/// <summary>Takes a payment and answers <c>true</c> when it went through, <c>false</c> when it was declined.</summary>
public sealed record TakePaymentCommand;

public sealed class TakePaymentCommandHandler(IPaymentGateway paymentGateway) : ICommandHandler<TakePaymentCommand, bool>
{
    public Task<bool> HandleAsync(TakePaymentCommand command, CancellationToken cancellationToken = default)
    {
        return paymentGateway.ChargeAsync(cancellationToken);
    }
}
