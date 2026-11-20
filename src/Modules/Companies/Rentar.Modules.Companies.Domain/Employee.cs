using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// An Identity user working at a company, with the roles that say what they may do there. A user is employed by a
/// company at most once, and may work at several companies. An employee always holds at least one role, and only
/// roles of their company; the constructor refuses others with <see cref="BusinessRuleException"/>.
/// </summary>
public class Employee : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads an employee.</summary>
    private Employee()
    {
    }

    public Employee(int companyId, int userId, IEnumerable<Role> roles)
    {
        var granted = roles.ToList();

        if (granted.Count == 0)
        {
            throw new BusinessRuleException(DomainMessages.EmployeeNeedsRole);
        }

        if (granted.Any(role => role.CompanyId != companyId))
        {
            throw new BusinessRuleException(DomainMessages.EmployeeRoleOfOtherCompany);
        }

        CompanyId = companyId;
        UserId = userId;
        Roles = granted;
    }

    public int Id { get; private init; }
    public int CompanyId { get; private init; }

    /// <summary>Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int UserId { get; private init; }

    public Company Company { get; private init; }

    /// <summary>Roles of the same company, granted through <see cref="EmployeeRole"/>.</summary>
    public ICollection<Role> Roles { get; private init; } = [];

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
