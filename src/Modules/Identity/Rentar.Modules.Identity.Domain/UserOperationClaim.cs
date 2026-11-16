using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Identity.Domain;

/// <summary>
/// Grants an operation claim to a user: the join of the many-to-many between <see cref="User"/> and
/// <see cref="OperationClaim"/>. The pair is its key, so a claim is granted to a user at most once.
/// </summary>
public class UserOperationClaim : IEntity
{
    public int UserId { get; init; }
    public int OperationClaimId { get; init; }
}
