namespace Rentar.Api.IntegrationTests;

/// <summary>
/// A color as the API sends and takes it. Tests use this rather than the domain's <c>Color</c>, which can only be
/// built valid, so they can send whatever a client might, invalid colors included.
/// </summary>
public sealed class ColorBody
{
    public int Id { get; init; }
    public string ColorName { get; init; }
}
