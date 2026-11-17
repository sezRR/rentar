namespace Rentar.Api.IntegrationTests;

/// <summary>A user as v1 sends it, rather than the domain's <c>User</c>.</summary>
public sealed class UserBody
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public byte[] PasswordHash { get; init; }
    public byte[] PasswordSalt { get; init; }
    public bool Status { get; init; }
}
