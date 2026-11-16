using Microsoft.AspNetCore.Http;
using Rentar.Shared.Application.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Rentar.Shared.Infrastructure.Security;

/// <summary>
/// Reads the caller from the JWT principal of the HTTP request being served.
/// </summary>
public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? UserId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.ClaimRoles() ?? (IReadOnlyCollection<string>)Array.Empty<string>();
}
