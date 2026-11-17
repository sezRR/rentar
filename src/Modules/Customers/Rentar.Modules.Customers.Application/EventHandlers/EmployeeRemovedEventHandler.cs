using Rentar.Modules.Companies.Contracts.Events;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.EventHandlers;

/// <summary>A company customer has to work at the company, so one who leaves it stops renting for it.</summary>
public sealed class EmployeeRemovedEventHandler(ICommandHandler<ClearCustomerCompanyCommand> clearCustomerCompany)
    : IIntegrationEventHandler<EmployeeRemovedEvent>
{
    public Task HandleAsync(EmployeeRemovedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return clearCustomerCompany.HandleAsync(
            new ClearCustomerCompanyCommand(integrationEvent.UserId, integrationEvent.CompanyId),
            cancellationToken);
    }
}
