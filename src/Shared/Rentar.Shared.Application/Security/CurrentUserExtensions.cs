using System;

namespace Rentar.Shared.Application.Security;

public static class CurrentUserExtensions
{
    /// <summary>
    /// The caller's user id, for endpoints that require authorization: authentication has already refused a request
    /// without a valid token, so a missing id is a bug rather than a refusal.
    /// </summary>
    public static int RequireUserId(this ICurrentUser currentUser)
    {
        return currentUser.UserId ?? throw new InvalidOperationException("The operation requires an authenticated caller.");
    }
}
