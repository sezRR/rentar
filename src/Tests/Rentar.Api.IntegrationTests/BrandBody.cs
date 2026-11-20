namespace Rentar.Api.IntegrationTests;

/// <summary>
/// A brand as the API sends and takes it. Tests use this rather than the domain's <c>Brand</c>, which can only be
/// built valid, so they can send whatever a client might, invalid brands included.
/// </summary>
public sealed class BrandBody
{
    public int Id { get; init; }
    public string BrandName { get; init; }
}
