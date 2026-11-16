using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// A named set of permissions within one company. Its name is unique in that company.
/// </summary>
public class Role : IEntity
{
    public int Id { get; init; }
    public int CompanyId { get; init; }
    public string Name { get; init; }

    /// <summary>Values from the fixed permission catalogue (<c>CompanyPermissions</c> in the module's contracts).</summary>
    public List<string> Permissions { get; init; } = [];

    /// <summary>One of the defaults every company starts with (Owner, Admin, Member), rather than one it created.</summary>
    public bool IsPredefined { get; init; }

    public Company Company { get; init; }

    public ICollection<Employee> Employees { get; init; } = [];
}
