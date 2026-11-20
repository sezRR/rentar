namespace Rentar.Modules.Fleet.Contracts;

public sealed record CarSummary(
    int CarId,
    int BrandId,
    string BrandName,
    int ColorId,
    string ColorName,
    int ModelYear,
    decimal DailyPrice,
    string Description);
