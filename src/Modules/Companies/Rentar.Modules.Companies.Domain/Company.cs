using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// An organisation registered on Rentar. Its people are <see cref="Employees"/>, and what each of them may do there
/// comes from the company's <see cref="Roles"/>.
/// </summary>
public class Company : IEntity
{
    public int Id { get; init; }
    public string Name { get; init; }

    /// <summary>Deleted together with the company.</summary>
    public ICollection<Employee> Employees { get; init; } = [];

    /// <summary>The predefined roles copied in when the company registered, plus any it creates. Deleted with it.</summary>
    public ICollection<Role> Roles { get; init; } = [];
}
