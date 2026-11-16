using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

public class Car : IEntity
{
    public int Id { get; init; }
    public int BrandId { get; init; }
    public int ColorId { get; init; }
    public int ModelYear { get; init; }
    public decimal DailyPrice { get; init; }
    public string Description { get; init; }
    public int MinimumFindeksRating { get; init; }

    public Brand Brand { get; init; }
    public Color Color { get; init; }

    /// <summary>At most five; they are deleted together with the car.</summary>
    public ICollection<CarImage> Images { get; init; } = [];
}
