namespace Rentar.Api.IntegrationTests;

/// <summary>A customer as the API sends and takes it, rather than the domain's <c>Customer</c>.</summary>
public sealed class CustomerBody
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int? CompanyId { get; init; }
}
