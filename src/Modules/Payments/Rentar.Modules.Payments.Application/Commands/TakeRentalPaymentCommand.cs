using Rentar.Modules.Payments.Application.Common.Interfaces;
using Rentar.Modules.Payments.Contracts.Events;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application.Commands;

/// <summary>
/// Takes the payment for the Rentals rental <see cref="RentalId"/> and publishes the outcome:
/// <see cref="PaymentSucceededEvent"/> or <see cref="PaymentDeclinedEvent"/>.
/// </summary>
public sealed record TakeRentalPaymentCommand(int RentalId);

public sealed class TakeRentalPaymentCommandHandler(IPaymentGateway paymentGateway, IEventPublisher eventPublisher)
    : ICommandHandler<TakeRentalPaymentCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(TakeRentalPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (await paymentGateway.ChargeAsync(cancellationToken))
        {
            await eventPublisher.PublishAsync(new PaymentSucceededEvent(command.RentalId), cancellationToken);
        }
        else
        {
            await eventPublisher.PublishAsync(new PaymentDeclinedEvent(command.RentalId), cancellationToken);
        }
    }
}
