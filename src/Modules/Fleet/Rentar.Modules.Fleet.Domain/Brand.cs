using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

/// <summary>A brand can only be built with a valid name; the constructor refuses others with <see cref="BusinessRuleException"/>.</summary>
public class Brand : IEntity
{
    public const int NameMinLength = 2;

    /// <summary>For EF Core, which sets the stored values itself when it reads a brand.</summary>
    private Brand()
    {
    }

    public Brand(string brandName)
    {
        if (string.IsNullOrWhiteSpace(brandName) || brandName.Length < NameMinLength)
        {
            throw new BusinessRuleException(DomainMessages.BrandNameTooShort);
        }

        BrandName = brandName;
    }

    /// <summary>Set by the database; a use case that replaces a stored brand sets it to say which one.</summary>
    public int Id { get; init; }

    public string BrandName { get; private init; }

    /// <summary>The cars of this brand. A brand can only be deleted once it has none.</summary>
    public ICollection<Car> Cars { get; private init; } = [];
}
