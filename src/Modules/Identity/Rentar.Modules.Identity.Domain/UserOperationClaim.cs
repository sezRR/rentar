using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Identity.Domain;

public class UserOperationClaim : IEntity
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int OperationClaimId { get; init; }
}
