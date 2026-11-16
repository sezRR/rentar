using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Findeks.Application.Common.Interfaces;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application.Commands;

/// <summary>
/// Calculates the customer's Findeks rating and stores it as the customer's score, replacing an earlier one; answers
/// the stored score. Throws <see cref="NotFoundException"/> when the Customers module does not know the customer.
/// </summary>
public sealed record CalculateCustomerScoreCommand(int CustomerId);

public sealed class CalculateCustomerScoreCommandHandler(ICustomerScoreDal customerScoreDal, ICustomersModuleApi customersModule)
    : ICommandHandler<CalculateCustomerScoreCommand, CustomerScore>
{
    public async Task<CustomerScore> HandleAsync(CalculateCustomerScoreCommand command, CancellationToken cancellationToken = default)
    {
        // The customer lives in another module, so no foreign key guards the reference; that module is asked instead.
        var customers = await customersModule.GetCustomersAsync([command.CustomerId], cancellationToken);
        if (customers.Count == 0)
        {
            throw new NotFoundException(Messages.CustomerNotFound(command.CustomerId));
        }

        var score = new CustomerScore { CustomerId = command.CustomerId, FindeksRating = FindeksRating.Calculate() };

        if (await customerScoreDal.AnyAsync(s => s.CustomerId == command.CustomerId, cancellationToken))
        {
            await customerScoreDal.UpdateAsync(score, cancellationToken);
        }
        else
        {
            await customerScoreDal.AddAsync(score, cancellationToken);
        }

        return score;
    }
}
