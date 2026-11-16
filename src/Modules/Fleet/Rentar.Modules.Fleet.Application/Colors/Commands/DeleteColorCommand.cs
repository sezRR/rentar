using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Commands;

/// <summary>Deletes the color; throws <see cref="ConflictException"/> while a car still has it.</summary>
public sealed record DeleteColorCommand(int Id);

public sealed class DeleteColorCommandHandler(IColorDal colorDal) : ICommandHandler<DeleteColorCommand>
{
    public async Task HandleAsync(DeleteColorCommand command, CancellationToken cancellationToken = default)
    {
        // The foreign key refuses it too; asking first turns that into an answer the caller can act on.
        if (await colorDal.AnyAsync(c => c.Id == command.Id && c.Cars.Any(), cancellationToken))
        {
            throw new ConflictException(Messages.ColorHasCars);
        }

        await colorDal.DeleteAsync(new Color { Id = command.Id }, cancellationToken);
    }
}
