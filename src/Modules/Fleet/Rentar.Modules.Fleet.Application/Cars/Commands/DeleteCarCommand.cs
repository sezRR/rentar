using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Modules.Rentals.Contracts;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Commands;

/// <summary>
/// Deletes the car together with its images and their files, all or nothing; throws <see cref="ConflictException"/>
/// while a rental is of it.
/// </summary>
public sealed record DeleteCarCommand(int Id);

public sealed class DeleteCarCommandHandler(ICarDal carDal, ICarImageDal carImageDal, IImageStorage imageStorage, IRentalsModuleApi rentalsModule)
    : ICommandHandler<DeleteCarCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(DeleteCarCommand command, CancellationToken cancellationToken = default)
    {
        // Rentals live in another module, so no foreign key keeps their car in place; that module is asked instead.
        if (await rentalsModule.HasRentalsAsync(command.Id, cancellationToken))
        {
            throw new ConflictException(Messages.CarHasRentals);
        }

        var images = await carImageDal.GetAllAsync(c => c.CarId == command.Id, cancellationToken);

        // The database deletes the car's image rows with it (cascade); their files are deleted here. The file storage
        // takes part in the transaction, so the files only go once the car is gone for good.
        await carDal.DeleteAsync(new Car { Id = command.Id }, cancellationToken);

        foreach (var image in images)
        {
            imageStorage.Delete(image.ImagePath);
        }
    }
}
