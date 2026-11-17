using Rentar.Modules.Identity.Contracts.Events;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.EventHandlers;

/// <summary>A deactivated user will not come for their cars, so the rentals they have not started free them.</summary>
public sealed class UserDeactivatedEventHandler(ICommandHandler<CancelUpcomingRentalsOfUserCommand> cancelUpcomingRentals)
    : IIntegrationEventHandler<UserDeactivatedEvent>
{
    public Task HandleAsync(UserDeactivatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return cancelUpcomingRentals.HandleAsync(new CancelUpcomingRentalsOfUserCommand(integrationEvent.UserId), cancellationToken);
    }
}
