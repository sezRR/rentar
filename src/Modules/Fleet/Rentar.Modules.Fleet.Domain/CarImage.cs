using Rentar.Shared.Kernel.Entities;
using System;

namespace Rentar.Modules.Fleet.Domain;

public class CarImage : IEntity
{
    public int Id { get; init; }
    public int CarId { get; init; }
    public string ImagePath { get; init; }
    public DateTime Date { get; init; }
}
