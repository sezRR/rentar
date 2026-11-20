using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>Deletes the image and its file; throws <see cref="NotFoundException"/> when there is no such image.</summary>
public sealed record DeleteCarImageCommand(int Id);

public sealed class DeleteCarImageCommandHandler(ICarImageDal carImageDal, IImageStorage imageStorage) : ICommandHandler<DeleteCarImageCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(DeleteCarImageCommand command, CancellationToken cancellationToken = default)
    {
        var carImage = await carImageDal.GetAsync(c => c.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException(Messages.CarImageNotFound);

        await carImageDal.DeleteAsync(carImage, cancellationToken);
        imageStorage.Delete(carImage.ImagePath);
    }
}
