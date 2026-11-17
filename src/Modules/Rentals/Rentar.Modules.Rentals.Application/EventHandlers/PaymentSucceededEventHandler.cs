using Rentar.Modules.Payments.Contracts.Events;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.EventHandlers;

public sealed class PaymentSucceededEventHandler(ICommandHandler<ConfirmRentalCommand> confirmRental)
    : IIntegrationEventHandler<PaymentSucceededEvent>
{
    public Task HandleAsync(PaymentSucceededEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return confirmRental.HandleAsync(new ConfirmRentalCommand(integrationEvent.RentalId), cancellationToken);
    }
}
