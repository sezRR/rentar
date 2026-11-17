using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Contracts;
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
/// Removes the employee <see cref="EmployeeId"/> from the company on behalf of its employee <see cref="ActorUserId"/>,
/// who may be removing themselves, and publishes <see cref="EmployeeRemovedEvent"/>.
/// </summary>
/// <remarks>
/// The actor needs <see cref="CompanyPermissions.EmployeesManage"/> and every permission the employee holds
/// (<see cref="ForbiddenException"/>), so an Admin can not remove an Owner. Throws <see cref="NotFoundException"/> for an
/// employee of another company, and <see cref="BusinessRuleException"/> when the employee is the last one holding every
/// permission, which would leave nobody able to run the company.
/// </remarks>
public sealed record RemoveEmployeeCommand(int ActorUserId, int CompanyId, int EmployeeId);

public sealed class RemoveEmployeeCommandHandler(IEmployeeDal employeeDal, IEventPublisher eventPublisher) : ICommandHandler<RemoveEmployeeCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(RemoveEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await employeeDal.RequirePermissionAsync(
            command.CompanyId, command.ActorUserId, CompanyPermissions.EmployeesManage, cancellationToken);

        var employees = await employeeDal.GetByCompanyAsync(command.CompanyId, cancellationToken);
        var employee = employees.SingleOrDefault(e => e.Id == command.EmployeeId)
            ?? throw new NotFoundException(Messages.EmployeeNotFound(command.EmployeeId));

        if (!employee.Permissions().All(actor.HasPermission))
        {
            throw new ForbiddenException(Messages.CannotRemoveEmployeeWithMorePermissions);
        }

        if (FullAccess.IsLastHeldBy(employee, employees))
        {
            throw new BusinessRuleException(Messages.CompanyNeedsFullAccessEmployee);
        }

        // A fresh entity without its loaded roles; the database removes the role grants with the employee.
        await employeeDal.DeleteAsync(
            new Employee { Id = employee.Id, CompanyId = employee.CompanyId, UserId = employee.UserId },
            cancellationToken);

        await eventPublisher.PublishAsync(new EmployeeRemovedEvent(employee.CompanyId, employee.UserId), cancellationToken);
    }
}
