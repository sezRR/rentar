using Rentar.Shared.Kernel.Entities;
using System;

namespace Rentar.Modules.Fleet.Domain;

/// <summary>
/// An image is only made by its car (<see cref="Domain.Car.NewImage"/>), which keeps it within the car's limit.
/// </summary>
public class CarImage : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads an image.</summary>
    private CarImage()
    {
    }

    internal CarImage(int carId, string imagePath, DateTime date)
    {
        CarId = carId;
        ImagePath = imagePath;
        Date = date;
    }

    public int Id { get; private init; }
    public int CarId { get; private init; }
    public string ImagePath { get; private init; }
    public DateTime Date { get; private init; }

    public Car Car { get; private init; }

    /// <summary>This image, of the same car, with another file uploaded at <paramref name="date"/>.</summary>
    public CarImage WithFile(string imagePath, DateTime date)
    {
        return new CarImage(CarId, imagePath, date) { Id = Id };
    }
}
