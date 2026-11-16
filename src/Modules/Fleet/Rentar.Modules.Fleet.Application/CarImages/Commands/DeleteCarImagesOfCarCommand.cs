using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>Deletes every image of the car and their files, all or nothing. A car without images is left as it is.</summary>
public sealed record DeleteCarImagesOfCarCommand(int CarId);

public sealed class DeleteCarImagesOfCarCommandHandler(ICarImageDal carImageDal, IImageStorage imageStorage) : ICommandHandler<DeleteCarImagesOfCarCommand>
{
    [TransactionScopeAspect]
    public void Handle(DeleteCarImagesOfCarCommand command)
    {
        foreach (var carImage in carImageDal.GetAll(c => c.CarId == command.CarId))
        {
            carImageDal.Delete(carImage);
            imageStorage.Delete(carImage.ImagePath);
        }
    }
}
