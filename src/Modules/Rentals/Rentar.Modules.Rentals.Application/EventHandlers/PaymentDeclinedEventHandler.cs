using Rentar.Modules.Payments.Contracts.Events;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.EventHandlers;

public sealed class PaymentDeclinedEventHandler(ICommandHandler<CancelRentalCommand> cancelRental)
    : IIntegrationEventHandler<PaymentDeclinedEvent>
{
    public Task HandleAsync(PaymentDeclinedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return cancelRental.HandleAsync(new CancelRentalCommand(integrationEvent.RentalId), cancellationToken);
    }
}
