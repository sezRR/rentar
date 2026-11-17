using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Modules.Companies.Application.Employees;

/// <summary>
/// A company always keeps an employee who holds every permission, so someone can still run it. Checked by every use
/// case that takes an employee away. Reads the employees' roles, so they must be loaded.
/// </summary>
internal static class FullAccess
{
    public static bool IsHeldBy(Employee employee)
    {
        return CompanyPermissions.All.All(employee.HasPermission);
    }

    /// <summary>
    /// Whether <paramref name="employee"/> is the only one of the company's <paramref name="employees"/> holding every
    /// permission, so the company would be left without anyone able to run it once they go.
    /// </summary>
    public static bool IsLastHeldBy(Employee employee, IEnumerable<Employee> employees)
    {
        return IsHeldBy(employee) && !employees.Any(other => other.Id != employee.Id && IsHeldBy(other));
    }
}
