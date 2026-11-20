using Rentar.Modules.Payments.Application.Commands;
using Rentar.Modules.Rentals.Contracts.Events;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application.EventHandlers;

/// <summary>A booked rental is paid for right away; Rentals hears the outcome back.</summary>
public sealed class RentalCreatedEventHandler(ICommandHandler<TakeRentalPaymentCommand> takeRentalPayment)
    : IIntegrationEventHandler<RentalCreatedEvent>
{
    public Task HandleAsync(RentalCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return takeRentalPayment.HandleAsync(new TakeRentalPaymentCommand(integrationEvent.RentalId), cancellationToken);
    }
}
