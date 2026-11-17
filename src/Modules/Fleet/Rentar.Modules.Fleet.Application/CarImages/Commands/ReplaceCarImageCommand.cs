using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>
/// Replaces the stored file of image <see cref="Id"/> with <see cref="File"/>; the image stays with its car. Throws
/// <see cref="BusinessRuleException"/> when the file is missing or not a PNG or JPEG, and
/// <see cref="NotFoundException"/> when there is no such image.
/// </summary>
public sealed record ReplaceCarImageCommand(int Id, FileUpload File);

public sealed class ReplaceCarImageCommandHandler(ICarImageDal carImageDal, IImageStorage imageStorage, TimeProvider timeProvider)
    : ICommandHandler<ReplaceCarImageCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(ReplaceCarImageCommand command, CancellationToken cancellationToken = default)
    {
        if (!await ImageFileValidator.IsSupportedImageAsync(command.File, cancellationToken))
        {
            throw new BusinessRuleException(Messages.CarImageInvalidFile);
        }

        // The stored path is used, not one sent by the client, so only this image's file is replaced.
        var existing = await carImageDal.GetAsync(c => c.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException(Messages.CarImageNotFound);

        var imagePath = await imageStorage.ReplaceAsync(command.File, existing.ImagePath, cancellationToken);

        await carImageDal.UpdateAsync(existing.WithFile(imagePath, timeProvider.GetLocalNow().DateTime), cancellationToken);
    }
}
