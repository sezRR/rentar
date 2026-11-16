using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// An Identity user working at a company, with the roles that say what they may do there. A user is employed by a
/// company at most once, and may work at several companies.
/// </summary>
public class Employee : IEntity
{
    public int Id { get; init; }
    public int CompanyId { get; init; }

    /// <summary>Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int UserId { get; init; }

    public Company Company { get; init; }

    /// <summary>Roles of the same company, granted through <see cref="EmployeeRole"/>.</summary>
    public ICollection<Role> Roles { get; init; } = [];

    /// <summary>Every permission the employee's roles grant. Reads <see cref="Roles"/>, so they must be loaded.</summary>
    public IReadOnlySet<string> Permissions()
    {
        return Roles.SelectMany(role => role.Permissions).ToHashSet();
    }

    /// <summary>Whether one of the employee's roles grants <paramref name="permission"/>. Reads <see cref="Roles"/>.</summary>
    public bool HasPermission(string permission)
    {
        return Roles.Any(role => role.Permissions.Contains(permission));
    }
}
