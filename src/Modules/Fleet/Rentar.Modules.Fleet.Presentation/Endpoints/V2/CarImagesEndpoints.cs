#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.CarImages;
using Rentar.Modules.Fleet.Application.CarImages.Commands;
using Rentar.Modules.Fleet.Application.CarImages.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

/// <summary>
/// Images belong to a car, so they are addressed under it: <c>/cars/{carId}/images</c>.
/// Only listing every image of every car has its own top-level collection, <c>/car-images</c>.
/// </summary>
public class CarImagesEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var images = builder.MapVersionedGroup("CarImages", "cars/{carId:int}/images", 2.0);

        images.MapGet("/", GetByCar)
            .ProducesValidationProblem();
        images.MapGet("/{imageId:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        images.MapPost("/", Upload)
            .DisableAntiforgery()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        images.MapPut("/{imageId:int}", Replace)
            .DisableAntiforgery()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        images.MapDelete("/{imageId:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound);
        images.MapDelete("/", DeleteAll);

        builder.MapVersionedGroup("CarImages", "car-images", 2.0)
            .MapGet("/", GetAll)
            .ProducesValidationProblem();
    }

    private static async Task<Ok<PagedResponse<CarImage>>> GetAll(
        IQueryHandler<GetCarImagesQuery, Page<CarImage>> getCarImages,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getCarImages.HandleAsync(new GetCarImagesQuery(paging.ToRequest()), cancellationToken));
    }

    private static async Task<Ok<PagedResponse<CarImage>>> GetByCar(
        IQueryHandler<GetCarImagesByCarQuery, Page<CarImage>> getCarImagesByCar,
        int carId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getCarImagesByCar.HandleAsync(new GetCarImagesByCarQuery(carId, paging.ToRequest()), cancellationToken));
    }

    private static async Task<Results<Ok<CarImage>, ProblemHttpResult>> GetById(
        IQueryHandler<GetCarImageByIdQuery, CarImage> getCarImageById,
        int carId,
        int imageId,
        CancellationToken cancellationToken)
    {
        var carImage = await FindAsync(getCarImageById, carId, imageId, cancellationToken);
        return carImage is null ? RestResults.NotFound("Car image", imageId) : TypedResults.Ok(carImage);
    }

    /// <summary>
    /// Stores a PNG or JPEG sent as the <c>file</c> form field; without one the car gets the default image. A car that
    /// is not stored answers 404.
    /// </summary>
    private static async Task<Created<CarImage>> Upload(
        ICommandHandler<AddCarImageCommand, CarImage> addCarImage,
        HttpRequest request,
        int carId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var carImage = await addCarImage.HandleAsync(new AddCarImageCommand(carId, ToUpload(file)), cancellationToken);
        return RestResults.Created(request, carImage.Id, carImage);
    }

    /// <summary>
    /// Replaces the image's file with the PNG or JPEG sent as the <c>file</c> form field.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Replace(
        IQueryHandler<GetCarImageByIdQuery, CarImage> getCarImageById,
        ICommandHandler<ReplaceCarImageCommand> replaceCarImage,
        int carId,
        int imageId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (await FindAsync(getCarImageById, carId, imageId, cancellationToken) is null)
        {
            return RestResults.NotFound("Car image", imageId);
        }

        // A missing file is turned down by the use case with the same message as an unsupported one.
        await replaceCarImage.HandleAsync(new ReplaceCarImageCommand(imageId, ToUpload(file)), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
        IQueryHandler<GetCarImageByIdQuery, CarImage> getCarImageById,
        ICommandHandler<DeleteCarImageCommand> deleteCarImage,
        int carId,
        int imageId,
        CancellationToken cancellationToken)
    {
        if (await FindAsync(getCarImageById, carId, imageId, cancellationToken) is null)
        {
            return RestResults.NotFound("Car image", imageId);
        }

        await deleteCarImage.HandleAsync(new DeleteCarImageCommand(imageId), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes every image of the car. A car without images is already in that state, so this also answers 204.
    /// </summary>
    private static async Task<NoContent> DeleteAll(
        ICommandHandler<DeleteCarImagesOfCarCommand> deleteCarImagesOfCar,
        int carId,
        CancellationToken cancellationToken)
    {
        await deleteCarImagesOfCar.HandleAsync(new DeleteCarImagesOfCarCommand(carId), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// An image is only found under the car it belongs to.
    /// </summary>
    private static async Task<CarImage?> FindAsync(
        IQueryHandler<GetCarImageByIdQuery, CarImage> getCarImageById,
        int carId,
        int imageId,
        CancellationToken cancellationToken)
    {
        var carImage = await getCarImageById.HandleAsync(new GetCarImageByIdQuery(imageId), cancellationToken);
        return carImage?.CarId == carId ? carImage : null;
    }

    private static FileUpload? ToUpload(IFormFile? file)
    {
        return file is null ? null : new FileUpload(file.FileName, file.Length, file.OpenReadStream);
    }
}
