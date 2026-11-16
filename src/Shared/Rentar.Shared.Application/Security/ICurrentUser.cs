using System.Collections.Generic;

namespace Rentar.Shared.Application.Security;

/// <summary>
/// The caller of the current operation, so application code can authorize without knowing about HTTP.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The Identity user the access token was issued to, or <c>null</c> for an anonymous caller.</summary>
    int? UserId { get; }

    IReadOnlyCollection<string> Roles { get; }
}
