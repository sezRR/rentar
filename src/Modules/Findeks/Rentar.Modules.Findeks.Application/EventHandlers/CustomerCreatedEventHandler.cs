using Rentar.Modules.Customers.Contracts.Events;
using Rentar.Modules.Findeks.Application.Commands;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application.EventHandlers;

/// <summary>Every customer starts with a Findeks rating, rather than having none until one is asked for.</summary>
public sealed class CustomerCreatedEventHandler(ICommandHandler<CalculateCustomerScoreCommand, CustomerScore> calculateCustomerScore)
    : IIntegrationEventHandler<CustomerCreatedEvent>
{
    public Task HandleAsync(CustomerCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return calculateCustomerScore.HandleAsync(new CalculateCustomerScoreCommand(integrationEvent.CustomerId), cancellationToken);
    }
}
