namespace Rentar.Api.IntegrationTests;

/// <summary>A company as the API sends it, rather than the domain's <c>Company</c>.</summary>
public sealed class CompanyBody
{
    public int Id { get; init; }
    public string Name { get; init; }
}
