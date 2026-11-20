using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Domain;

/// <summary>A color can only be built with a valid name; the constructor refuses others with <see cref="BusinessRuleException"/>.</summary>
public class Color : IEntity
{
    public const int NameMinLength = 2;

    /// <summary>For EF Core, which sets the stored values itself when it reads a color.</summary>
    private Color()
    {
    }

    public Color(string colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName) || colorName.Length < NameMinLength)
        {
            throw new BusinessRuleException(DomainMessages.ColorNameInvalid);
        }

        ColorName = colorName;
    }

    /// <summary>Set by the database; a use case that replaces a stored color sets it to say which one.</summary>
    public int Id { get; init; }

    public string ColorName { get; private init; }

    /// <summary>The cars of this color. A color can only be deleted once it has none.</summary>
    public ICollection<Car> Cars { get; private init; } = [];
}
