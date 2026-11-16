namespace Rentar.Modules.Identity.Contracts;

/// <param name="IsActive">The user's <c>Status</c>; false for a user stored as inactive.</param>
public sealed record UserSummary(int UserId, string FirstName, string LastName, bool IsActive);
