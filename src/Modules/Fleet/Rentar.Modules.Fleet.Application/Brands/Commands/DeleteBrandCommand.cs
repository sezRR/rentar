using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Commands;

/// <summary>Deletes the brand; throws <see cref="ConflictException"/> while a car still has it.</summary>
public sealed record DeleteBrandCommand(int Id);

public sealed class DeleteBrandCommandHandler(IBrandDal brandDal) : ICommandHandler<DeleteBrandCommand>
{
    public async Task HandleAsync(DeleteBrandCommand command, CancellationToken cancellationToken = default)
    {
        // The foreign key refuses it too; asking first turns that into an answer the caller can act on.
        if (await brandDal.AnyAsync(b => b.Id == command.Id && b.Cars.Any(), cancellationToken))
        {
            throw new ConflictException(Messages.BrandHasCars);
        }

        await brandDal.DeleteAsync(new Brand { Id = command.Id }, cancellationToken);
    }
}
