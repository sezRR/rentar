using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// Grants a role to an employee: the join of the many-to-many between <see cref="Employee"/> and <see cref="Role"/>.
/// The pair is its key, so a role is granted to an employee at most once.
/// </summary>
public class EmployeeRole : IEntity
{
    public int EmployeeId { get; init; }
    public int RoleId { get; init; }
}
