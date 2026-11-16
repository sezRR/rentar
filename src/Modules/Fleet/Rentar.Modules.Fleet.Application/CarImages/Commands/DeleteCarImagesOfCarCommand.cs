using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>Deletes every image of the car and their files, all or nothing. A car without images is left as it is.</summary>
public sealed record DeleteCarImagesOfCarCommand(int CarId);

public sealed class DeleteCarImagesOfCarCommandHandler(ICarImageDal carImageDal, IImageStorage imageStorage) : ICommandHandler<DeleteCarImagesOfCarCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(DeleteCarImagesOfCarCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var carImage in await carImageDal.GetAllAsync(c => c.CarId == command.CarId, cancellationToken))
        {
            await carImageDal.DeleteAsync(carImage, cancellationToken);
            imageStorage.Delete(carImage.ImagePath);
        }
    }
}
