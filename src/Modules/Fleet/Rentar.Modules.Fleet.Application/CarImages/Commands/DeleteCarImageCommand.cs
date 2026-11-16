using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>Deletes the image and its file; throws <see cref="NotFoundException"/> when there is no such image.</summary>
public sealed record DeleteCarImageCommand(int Id);

public sealed class DeleteCarImageCommandHandler(ICarImageDal carImageDal, IImageStorage imageStorage) : ICommandHandler<DeleteCarImageCommand>
{
    [TransactionScopeAspect]
    public void Handle(DeleteCarImageCommand command)
    {
        var carImage = carImageDal.Get(c => c.Id == command.Id)
            ?? throw new NotFoundException(Messages.CarImageNotFound);

        carImageDal.Delete(carImage);
        imageStorage.Delete(carImage.ImagePath);
    }
}
