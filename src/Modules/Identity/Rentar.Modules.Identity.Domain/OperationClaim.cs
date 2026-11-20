using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Identity.Domain;

public class OperationClaim : IEntity
{
    public int Id { get; init; }

    /// <summary>Unique; carried in access tokens as a role.</summary>
    public string Name { get; init; }

    /// <summary>The users the claim is granted to, linked through <see cref="UserOperationClaim"/>.</summary>
    public ICollection<User> Users { get; init; } = [];
}
