using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application;

/// <summary>
/// Who may act in a company. A caller who does not work there is told the company does not exist, so companies are
/// not revealed to outsiders; an employee whose roles lack the permission is refused.
/// </summary>
internal static class CompanyAccess
{
    /// <summary>
    /// The caller's employee record, roles and company included; throws <see cref="NotFoundException"/> when the caller
    /// does not work at the company.
    /// </summary>
    public static async Task<Employee> RequireEmployeeAsync(
        this IEmployeeDal employeeDal,
        int companyId,
        int userId,
        CancellationToken cancellationToken)
    {
        return await employeeDal.GetByUserAsync(companyId, userId, cancellationToken)
            ?? throw new NotFoundException(Messages.CompanyNotFound(companyId));
    }

    /// <summary>
    /// As <see cref="RequireEmployeeAsync"/>, and throws <see cref="ForbiddenException"/> when none of the caller's roles
    /// grants <paramref name="permission"/>.
    /// </summary>
    public static async Task<Employee> RequirePermissionAsync(
        this IEmployeeDal employeeDal,
        int companyId,
        int userId,
        string permission,
        CancellationToken cancellationToken)
    {
        var employee = await employeeDal.RequireEmployeeAsync(companyId, userId, cancellationToken);
        if (!employee.HasPermission(permission))
        {
            throw new ForbiddenException(Messages.PermissionMissing(permission));
        }

        return employee;
    }
}
