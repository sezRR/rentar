using FluentValidation;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;

namespace Rentar.Modules.Fleet.Application.Colors.Commands;

/// <summary>Stores a new color and answers it with its id set.</summary>
public sealed record CreateColorCommand(string ColorName);

public sealed class CreateColorCommandValidator : UseCaseValidator<CreateColorCommand>
{
    public CreateColorCommandValidator()
    {
        RuleFor(c => c.ColorName).NotEmpty().MinimumLength(2);
    }
}

public sealed class CreateColorCommandHandler(IColorDal colorDal) : ICommandHandler<CreateColorCommand, Color>
{
    public Color Handle(CreateColorCommand command)
    {
        var color = new Color { ColorName = command.ColorName };
        colorDal.Add(color);
        return color;
    }
}
