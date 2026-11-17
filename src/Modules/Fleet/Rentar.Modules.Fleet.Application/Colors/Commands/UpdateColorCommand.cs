using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Commands;

/// <summary>Replaces the color; throws <see cref="BusinessRuleException"/> when the name is shorter than two characters.</summary>
public sealed record UpdateColorCommand(int Id, string ColorName);

public sealed class UpdateColorCommandHandler(IColorDal colorDal) : ICommandHandler<UpdateColorCommand>
{
    public async Task HandleAsync(UpdateColorCommand command, CancellationToken cancellationToken = default)
    {
        // No validator on purpose: v1 has always answered a short name with the color's own refusal
        // (DomainMessages.ColorNameInvalid), and V1ContractTests pins it. A validator would answer with the
        // FluentValidation error body instead.
        var color = new Color(command.ColorName) { Id = command.Id };

        await colorDal.UpdateAsync(color, cancellationToken);
    }
}
