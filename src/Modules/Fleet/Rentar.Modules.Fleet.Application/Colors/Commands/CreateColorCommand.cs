using FluentValidation;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Commands;

/// <summary>Stores a new color and answers it with its id set.</summary>
public sealed record CreateColorCommand(string ColorName);

public sealed class CreateColorCommandValidator : UseCaseValidator<CreateColorCommand>
{
    public CreateColorCommandValidator()
    {
        RuleFor(c => c.ColorName).NotEmpty().MinimumLength(Color.NameMinLength);
    }
}

public sealed class CreateColorCommandHandler(IColorDal colorDal) : ICommandHandler<CreateColorCommand, Color>
{
    public async Task<Color> HandleAsync(CreateColorCommand command, CancellationToken cancellationToken = default)
    {
        var color = new Color(command.ColorName);
        await colorDal.AddAsync(color, cancellationToken);
        return color;
    }
}
