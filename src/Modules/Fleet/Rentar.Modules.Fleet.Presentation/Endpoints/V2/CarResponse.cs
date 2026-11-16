using Rentar.Modules.Fleet.Application.Common.Models;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

/// <summary>
/// A car as v2 returns it: its own fields together with the names of its brand and color and its first image.
/// </summary>
public sealed record CarResponse(
    int Id,
    int BrandId,
    string BrandName,
    int ColorId,
    string ColorName,
    int ModelYear,
    decimal DailyPrice,
    string Description,
    int MinimumFindeksRating,
    string ImagePath)
{
    internal static CarResponse From(CarDetailResponse car)
    {
        return new CarResponse(
            car.Id,
            car.BrandId,
            car.BrandName,
            car.ColorId,
            car.ColorName,
            car.ModelYear,
            car.DailyPrice,
            car.Description,
            car.MinimumFindeksRating,
            car.ImagePath);
    }
}
