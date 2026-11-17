namespace Rentar.Api.IntegrationTests;

/// <summary>A customer's scores as the API sends them, rather than the domain's <c>CustomerScore</c>.</summary>
public sealed class CustomerScoreBody
{
    public int CustomerId { get; init; }
    public int FindeksRating { get; init; }
}
