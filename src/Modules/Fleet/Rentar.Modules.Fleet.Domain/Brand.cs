using Rentar.Shared.Kernel.Entities;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

public class Brand : IEntity
{
    public int Id { get; init; }
    public string BrandName { get; init; }

    /// <summary>The cars of this brand. A brand can only be deleted once it has none.</summary>
    public ICollection<Car> Cars { get; init; } = [];
}
