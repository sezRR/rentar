using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Contracts.Events;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Employees.Commands;

/// <summary>
/// Removes the Identity user <see cref="UserId"/> from every company they work at, publishing
/// <see cref="EmployeeRemovedEvent"/> for each. Nobody acts for a company here: the user is gone from Identity's point
/// of view, e.g. deactivated, so no permission is checked.
/// </summary>
/// <remarks>
/// Throws <see cref="BusinessRuleException"/>, and removes nothing, when the user is the last employee holding every
/// permission at one of the companies; ask <c>ICompaniesModuleApi.IsLastFullAccessEmployeeAsync</c> first. Run for an
/// event, it is retried then, and succeeds once another employee holds every permission.
/// </remarks>
public sealed record RemoveUserFromCompaniesCommand(int UserId);

public sealed class RemoveUserFromCompaniesCommandHandler(IEmployeeDal employeeDal, IEventPublisher eventPublisher)
    : ICommandHandler<RemoveUserFromCompaniesCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(RemoveUserFromCompaniesCommand command, CancellationToken cancellationToken = default)
    {
        var employments = await employeeDal.GetAllAsync(e => e.UserId == command.UserId, cancellationToken);

        foreach (var employment in employments.OrderBy(e => e.CompanyId))
        {
            var employees = await employeeDal.GetByCompanyAsync(employment.CompanyId, cancellationToken);
            if (FullAccess.IsLastHeldBy(employees.Single(e => e.Id == employment.Id), employees))
            {
                throw new BusinessRuleException(Messages.CompanyNeedsFullAccessEmployee);
            }

            // The database removes the role grants with the employee.
            await employeeDal.DeleteAsync(
                new Employee { Id = employment.Id, CompanyId = employment.CompanyId, UserId = employment.UserId },
                cancellationToken);

            await eventPublisher.PublishAsync(new EmployeeRemovedEvent(employment.CompanyId, employment.UserId), cancellationToken);
        }
    }
}
