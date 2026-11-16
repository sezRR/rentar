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
        // The one input check outside a validator: v1 has always answered it with this message, and V1ContractTests
        // pins it. A validator would answer with the FluentValidation error body instead.
        if (command.ColorName is not { Length: >= 2 })
        {
            throw new BusinessRuleException(Messages.ColorNameInvalid);
        }

        await colorDal.UpdateAsync(new Color { Id = command.Id, ColorName = command.ColorName }, cancellationToken);
    }
}
