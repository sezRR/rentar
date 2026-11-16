using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Identity.Domain;

public class User : IEntity
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }

    /// <summary>Unique: no two users share an email.</summary>
    public string Email { get; init; }

    public byte[] PasswordHash { get; init; }
    public byte[] PasswordSalt { get; init; }
    public bool Status { get; init; }

    /// <summary>The operation claims granted to the user, linked through <see cref="UserOperationClaim"/>.</summary>
    public ICollection<OperationClaim> OperationClaims { get; init; } = [];
}
