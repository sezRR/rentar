using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Domain;
using System.Collections.Generic;

namespace Rentar.Modules.Companies.Application.Roles;

/// <summary>
/// The roles every company starts with. They are copied into the company when it registers, so a change here only
/// reaches companies registered afterwards; existing ones need a migration.
/// </summary>
internal static class PredefinedRoles
{
    /// <summary>Holds every permission. A company always keeps at least one employee with all of them.</summary>
    public static readonly PredefinedRole Owner = new("Owner", CompanyPermissions.All);

    /// <summary>Runs the company's people and sees its rentals, but can not change the company itself.</summary>
    public static readonly PredefinedRole Admin = new("Admin",
    [
        CompanyPermissions.EmployeesRead,
        CompanyPermissions.EmployeesManage,
        CompanyPermissions.RentalsRead
    ]);

    /// <summary>Belongs to the company, e.g. to rent as one of its customers, with no say over it.</summary>
    public static readonly PredefinedRole Member = new("Member", []);

    public static readonly IReadOnlyList<PredefinedRole> All = [Owner, Admin, Member];
}

internal sealed record PredefinedRole(string Name, string[] Permissions)
{
    /// <summary>A new role of this kind; the company it belongs to is set when the company is stored with it.</summary>
    public Role ToRole()
    {
        return new Role(Name, Permissions, isPredefined: true);
    }
}
