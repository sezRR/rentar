using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

/// <summary>
/// A car can only be built valid: the constructor refuses a car that breaks its invariants with
/// <see cref="BusinessRuleException"/>, and nothing outside it can set what it checks.
/// </summary>
public class Car : IEntity
{
    public const int DescriptionMinLength = 2;
    public const int MaxImages = 5;

    /// <summary>For EF Core, which sets the stored values itself when it reads a car.</summary>
    private Car()
    {
    }

    public Car(int brandId, int colorId, int modelYear, decimal dailyPrice, string description, int minimumFindeksRating)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length < DescriptionMinLength)
        {
            throw new BusinessRuleException(DomainMessages.CarDescriptionTooShort);
        }

        if (modelYear <= 0)
        {
            throw new BusinessRuleException(DomainMessages.CarModelYearInvalid);
        }

        if (dailyPrice <= 0)
        {
            throw new BusinessRuleException(DomainMessages.CarDailyPriceInvalid);
        }

        BrandId = brandId;
        ColorId = colorId;
        ModelYear = modelYear;
        DailyPrice = dailyPrice;
        Description = description;
        MinimumFindeksRating = minimumFindeksRating;
    }

    /// <summary>Set by the database; a use case that replaces a stored car sets it to say which one.</summary>
    public int Id { get; init; }

    public int BrandId { get; private init; }
    public int ColorId { get; private init; }
    public int ModelYear { get; private init; }
    public decimal DailyPrice { get; private init; }
    public string Description { get; private init; }
    public int MinimumFindeksRating { get; private init; }

    public Brand Brand { get; private init; }
    public Color Color { get; private init; }

    /// <summary>At most <see cref="MaxImages"/>; they are deleted together with the car.</summary>
    public ICollection<CarImage> Images { get; private init; } = [];

    /// <summary>
    /// Throws <see cref="BusinessRuleException"/> when the car already has <see cref="MaxImages"/> images. Reads
    /// <see cref="Images"/>, so the car must have been read with them.
    /// </summary>
    public void EnsureRoomForImage()
    {
        if (Images.Count >= MaxImages)
        {
            throw new BusinessRuleException(DomainMessages.CarImageLimitReached);
        }
    }

    /// <summary>
    /// A new image of this car, not yet stored; the car itself is left as it is. Throws
    /// <see cref="BusinessRuleException"/> when it has no room for another (see <see cref="EnsureRoomForImage"/>).
    /// </summary>
    public CarImage NewImage(string imagePath, DateTime date)
    {
        EnsureRoomForImage();
        return new CarImage(Id, imagePath, date);
    }
}
