using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Commands;

/// <summary>
/// Stores <see cref="File"/> as an image of the car, or the default image when there is no file, and answers the image
/// with its id and path set. Throws <see cref="NotFoundException"/> when there is no such car, and
/// <see cref="BusinessRuleException"/> when the car already has the most images it may have or the file is not a PNG
/// or JPEG.
/// </summary>
public sealed record AddCarImageCommand(int CarId, FileUpload File);

public sealed class AddCarImageCommandHandler(ICarDal carDal, ICarImageDal carImageDal, IImageStorage imageStorage, TimeProvider timeProvider)
    : ICommandHandler<AddCarImageCommand, CarImage>
{
    [TransactionScopeAspect]
    public async Task<CarImage> HandleAsync(AddCarImageCommand command, CancellationToken cancellationToken = default)
    {
        // An image belongs to a stored car; the foreign key refuses any other.
        var car = await carDal.GetWithImagesAsync(command.CarId, cancellationToken)
            ?? throw new NotFoundException(Messages.TheCarNotFound);

        // Asked before the file is looked at or stored; the car asks again when it makes the image.
        car.EnsureRoomForImage();

        // Adding without a file falls back to the default image.
        if (command.File != null && !await ImageFileValidator.IsSupportedImageAsync(command.File, cancellationToken))
        {
            throw new BusinessRuleException(Messages.CarImageInvalidFile);
        }

        var imagePath = command.File == null
            ? IImageStorage.DefaultImageName
            : await imageStorage.SaveAsync(command.File, cancellationToken);

        var carImage = car.NewImage(imagePath, timeProvider.GetLocalNow().DateTime);

        await carImageDal.AddAsync(carImage, cancellationToken);
        return carImage;
    }
}
