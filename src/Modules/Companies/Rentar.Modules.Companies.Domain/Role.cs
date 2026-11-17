using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// A named set of permissions within one company. Its name is unique in that company, and never blank; the
/// constructor refuses a blank one with <see cref="BusinessRuleException"/>.
/// </summary>
public class Role : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a role.</summary>
    private Role()
    {
    }

    /// <summary>A new role; the company it belongs to is set when the company is stored with it.</summary>
    public Role(string name, IEnumerable<string> permissions, bool isPredefined)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(DomainMessages.RoleNameMissing);
        }

        Name = name;
        Permissions = [.. permissions];
        IsPredefined = isPredefined;
    }

    public int Id { get; private init; }
    public int CompanyId { get; private init; }
    public string Name { get; private init; }

    /// <summary>Values from the fixed permission catalogue (<c>CompanyPermissions</c> in the module's contracts).</summary>
    public List<string> Permissions { get; private init; } = [];

    /// <summary>One of the defaults every company starts with (Owner, Admin, Member), rather than one it created.</summary>
    public bool IsPredefined { get; private init; }

    public Company Company { get; private init; }

    public ICollection<Employee> Employees { get; private init; } = [];
}
