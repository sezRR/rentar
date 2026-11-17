using Rentar.Modules.Companies.Application.Employees.Commands;
using Rentar.Modules.Identity.Contracts.Events;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.EventHandlers;

/// <summary>A deactivated user can not act for a company any more, so they stop working at all of them.</summary>
public sealed class UserDeactivatedEventHandler(ICommandHandler<RemoveUserFromCompaniesCommand> removeUserFromCompanies)
    : IIntegrationEventHandler<UserDeactivatedEvent>
{
    public Task HandleAsync(UserDeactivatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return removeUserFromCompanies.HandleAsync(new RemoveUserFromCompaniesCommand(integrationEvent.UserId), cancellationToken);
    }
}
