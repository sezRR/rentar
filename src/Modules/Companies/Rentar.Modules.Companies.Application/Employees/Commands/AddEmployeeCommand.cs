using FluentValidation;
using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Domain;
using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Employees.Commands;

/// <summary>
/// Employs the Identity user <see cref="UserId"/> at the company with the company's roles <see cref="RoleIds"/>, on
/// behalf of its employee <see cref="ActorUserId"/>, and answers the new employee.
/// </summary>
/// <remarks>
/// The actor needs <see cref="CompanyPermissions.EmployeesManage"/> and can only grant permissions they hold
/// themselves (<see cref="ForbiddenException"/>), so nobody hands out more than they have. Throws
/// <see cref="BusinessRuleException"/> for a user Identity does not know or holds inactive and for a role of another
/// company, and <see cref="ConflictException"/> when the user already works there.
/// </remarks>
public sealed record AddEmployeeCommand(int ActorUserId, int CompanyId, int UserId, List<int> RoleIds);

public sealed class AddEmployeeCommandValidator : UseCaseValidator<AddEmployeeCommand>
{
    public AddEmployeeCommandValidator() => RuleFor(e => e.RoleIds).NotEmpty();
}

public sealed class AddEmployeeCommandHandler(IEmployeeDal employeeDal, IRoleDal roleDal, IIdentityModuleApi identityModule)
    : ICommandHandler<AddEmployeeCommand, EmployeeResponse>
{
    public async Task<EmployeeResponse> HandleAsync(AddEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await employeeDal.RequirePermissionAsync(
            command.CompanyId, command.ActorUserId, CompanyPermissions.EmployeesManage, cancellationToken);

        var user = await identityModule.RequireActiveUserAsync(command.UserId, cancellationToken);

        // The unique index refuses it too; asking first turns that into an answer the caller can act on.
        if (await employeeDal.AnyAsync(e => e.CompanyId == command.CompanyId && e.UserId == command.UserId, cancellationToken))
        {
            throw new ConflictException(Messages.UserAlreadyEmployee);
        }

        var roleIds = command.RoleIds.Distinct().ToArray();
        var roles = await roleDal.GetAllAsync(r => r.CompanyId == command.CompanyId && roleIds.Contains(r.Id), cancellationToken);
        if (roles.Count != roleIds.Length)
        {
            throw new BusinessRuleException(Messages.RoleNotFound);
        }

        if (!roles.SelectMany(role => role.Permissions).All(actor.HasPermission))
        {
            throw new ForbiddenException(Messages.CannotGrantMissingPermissions);
        }

        var employee = new Employee { CompanyId = command.CompanyId, UserId = command.UserId, Roles = roles };
        await employeeDal.AddWithRolesAsync(employee, cancellationToken);

        return new EmployeeResponse(employee.Id, employee.CompanyId, employee.UserId, user.FirstName, user.LastName, roles);
    }
}
