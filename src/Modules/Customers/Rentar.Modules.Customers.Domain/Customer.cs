using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Customers.Domain;

/// <summary>
/// A customer account. What it points at lives in other modules, so the use cases that build one ask those modules
/// whether the user and the company are there; nothing outside the customer can change what it holds.
/// </summary>
public class Customer : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a customer.</summary>
    private Customer()
    {
    }

    public Customer(int userId, int? companyId)
    {
        UserId = userId;
        CompanyId = companyId;
    }

    public int Id { get; private init; }

    /// <summary>
    /// Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one. Unique: a user has
    /// at most one customer account.
    /// </summary>
    public int UserId { get; private init; }

    /// <summary>
    /// Id of the Companies company the customer rents for, or <c>null</c> for a customer with no partner company (the
    /// "other" choice). Its user is one of that company's employees. A reference across modules, so not a foreign key,
    /// but indexed like one.
    /// </summary>
    public int? CompanyId { get; private init; }

    /// <summary>The same customer, now with no partner company.</summary>
    public Customer WithoutCompany()
    {
        return new Customer(UserId, companyId: null) { Id = Id };
    }
}
