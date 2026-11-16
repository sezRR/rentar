using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Customers.Domain;

public class Customer : IEntity
{
    public int Id { get; init; }

    /// <summary>
    /// Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one. Unique: a user has
    /// at most one customer account.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Id of the Companies company the customer rents for, or <c>null</c> for a customer with no partner company (the
    /// "other" choice). Its user is one of that company's employees. A reference across modules, so not a foreign key,
    /// but indexed like one.
    /// </summary>
    public int? CompanyId { get; init; }
}
