using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

public class Color : IEntity
{
    public int Id { get; init; }
    public string ColorName { get; init; }

    /// <summary>The cars of this color. A color can only be deleted once it has none.</summary>
    public ICollection<Car> Cars { get; init; } = [];
}
