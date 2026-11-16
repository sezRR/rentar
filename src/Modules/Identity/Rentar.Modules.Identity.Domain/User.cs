using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Identity.Domain;

public class User : IEntity
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public byte[] PasswordHash { get; init; }
    public byte[] PasswordSalt { get; init; }
    public bool Status { get; init; }
    public int FindeksRating { get; init; }
}
