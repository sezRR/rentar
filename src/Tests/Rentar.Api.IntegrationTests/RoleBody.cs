using System.Collections.Generic;

namespace Rentar.Api.IntegrationTests;

/// <summary>A company's role as the API sends it, rather than the domain's <c>Role</c>.</summary>
public sealed class RoleBody
{
    public int Id { get; init; }
    public int CompanyId { get; init; }
    public string Name { get; init; }
    public List<string> Permissions { get; init; } = [];
    public bool IsPredefined { get; init; }
}
