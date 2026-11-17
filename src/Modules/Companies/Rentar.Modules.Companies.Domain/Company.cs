using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Modules.Companies.Domain;

/// <summary>
/// An organisation registered on Rentar. Its people are <see cref="Employees"/>, and what each of them may do there
/// comes from the company's <see cref="Roles"/>. It is registered with its roles and its owner, and refuses a blank
/// name with <see cref="BusinessRuleException"/>.
/// </summary>
public class Company : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a company.</summary>
    private Company()
    {
    }

    /// <summary>
    /// A company being registered with <paramref name="roles"/>, and its first employee, the user
    /// <paramref name="ownerUserId"/>, who is granted <paramref name="ownerRole"/>, one of those roles.
    /// </summary>
    public Company(string name, IReadOnlyCollection<Role> roles, int ownerUserId, Role ownerRole)
    {
        Name = ValidName(name);

        if (!roles.Contains(ownerRole))
        {
            throw new BusinessRuleException(DomainMessages.CompanyOwnerRoleNotOfCompany);
        }

        Roles = [.. roles];
        Employees = [new Employee(Id, ownerUserId, [ownerRole])];
    }

    public int Id { get; private init; }
    public string Name { get; private init; }

    /// <summary>Deleted together with the company.</summary>
    public ICollection<Employee> Employees { get; private init; } = [];

    /// <summary>The predefined roles copied in when the company registered, plus any it creates. Deleted with it.</summary>
    public ICollection<Role> Roles { get; private init; } = [];

    /// <summary>The same company under <paramref name="name"/>; its employees and roles are left out, as they stay as they are.</summary>
    public Company Renamed(string name)
    {
        return new Company { Id = Id, Name = ValidName(name) };
    }

    private static string ValidName(string name)
    {
        return string.IsNullOrWhiteSpace(name) ? throw new BusinessRuleException(DomainMessages.CompanyNameMissing) : name;
    }
}
