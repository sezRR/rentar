using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Customers.Domain;

public class Customer : IEntity
{
    public int Id { get; init; }

    /// <summary>Id of the Identity user; a reference across modules, so not a foreign key, but indexed like one.</summary>
    public int UserId { get; init; }

    public string CompanyName { get; init; }
}
