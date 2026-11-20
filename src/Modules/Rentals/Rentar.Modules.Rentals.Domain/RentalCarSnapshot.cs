namespace Rentar.Modules.Rentals.Domain;

/// <summary>
/// The Fleet car as it was when the rental was booked. Later changes to the car, its brand or its color, such as a
/// renamed color or the car moved to another brand, leave it as it was.
/// </summary>
public sealed record RentalCarSnapshot(
    int BrandId,
    string BrandName,
    int ColorId,
    string ColorName,
    int ModelYear,
    string Description);
