using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Customers.Domain;

public class Customer : IEntity
{
    public int Id { get; init; }

    /// <summary>Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int UserId { get; init; }

    /// <summary>Free text the customer rents under, if any; defaults to the company's name for a company customer.</summary>
    public string? CompanyName { get; init; }

    /// <summary>
    /// Id of the Companies company the customer rents for, if any; its user is one of that company's employees. A
    /// reference across modules, so not a foreign key, but indexed like one.
    /// </summary>
    public int? CompanyId { get; init; }
}
