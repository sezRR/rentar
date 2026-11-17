namespace Rentar.Api.IntegrationTests;

/// <summary>
/// A car as v1 sends and takes it. Tests use this rather than the domain's <c>Car</c>, which can only be built
/// valid, so they can send whatever a client might, invalid cars included.
/// </summary>
public sealed class CarBody
{
    public int Id { get; init; }
    public int BrandId { get; init; }
    public int ColorId { get; init; }
    public int ModelYear { get; init; }
    public decimal DailyPrice { get; init; }
    public string Description { get; init; }
    public int MinimumFindeksRating { get; init; }
}
