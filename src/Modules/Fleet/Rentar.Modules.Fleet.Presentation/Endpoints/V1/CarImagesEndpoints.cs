using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.CarImages;
using Rentar.Modules.Fleet.Application.CarImages.Commands;
using Rentar.Modules.Fleet.Application.CarImages.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V1;

public class CarImagesEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("CarImages", "carimages", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getbyid", GetById);
        group.MapGet("/getbycarid", GetByCarId);
        group.MapPost("/add", Add).DisableAntiforgery();
        group.MapPost("/update", Update).DisableAntiforgery();
        group.MapDelete("/delete", Delete);
    }

    private static async Task<Ok<V1Response<List<CarImage>>>> GetAll(
        IQueryHandler<GetCarImagesQuery, List<CarImage>> getCarImages,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarImages.HandleAsync(new GetCarImagesQuery(), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<CarImage>>> GetById(
        IQueryHandler<GetCarImageByIdQuery, CarImage> getCarImageById,
        int id,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarImageById.HandleAsync(new GetCarImageByIdQuery(id), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<List<CarImage>>>> GetByCarId(
        IQueryHandler<GetCarImagesByCarQuery, List<CarImage>> getCarImagesByCar,
        int carId,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarImagesByCar.HandleAsync(new GetCarImagesByCarQuery(carId), cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Add(
        ICommandHandler<AddCarImageCommand, CarImage> addCarImage,
        [FromForm(Name = "carId")] string carId,
        [FromForm(Name = "file")] IFormFile file,
        CancellationToken cancellationToken)
    {
        await addCarImage.HandleAsync(new AddCarImageCommand(Convert.ToInt32(carId), ToUpload(file)), cancellationToken);
        return V1Results.Ok(null);
    }

    private static async Task<Ok<V1Response>> Update(
        ICommandHandler<ReplaceCarImageCommand> replaceCarImage,
        [FromForm(Name = "objectFile")] IFormFile objectFile,
        [FromForm(Name = "carImage")] CarImageForm carImage,
        CancellationToken cancellationToken)
    {
        await replaceCarImage.HandleAsync(new ReplaceCarImageCommand(carImage.Id, ToUpload(objectFile)), cancellationToken);
        return V1Results.Ok(null);
    }

    /// <summary>
    /// Despite its name, <paramref name="id"/> is a car id: every image of that car is deleted.
    /// </summary>
    private static async Task<Results<Ok<V1Response>, BadRequest<string>>> Delete(
        IQueryHandler<GetCarImagesByCarQuery, List<CarImage>> getCarImagesByCar,
        ICommandHandler<DeleteCarImagesOfCarCommand> deleteCarImagesOfCar,
        int id,
        CancellationToken cancellationToken)
    {
        if ((await getCarImagesByCar.HandleAsync(new GetCarImagesByCarQuery(id), cancellationToken)).Count == 0)
        {
            return TypedResults.BadRequest("This car do not have image!");
        }

        await deleteCarImagesOfCar.HandleAsync(new DeleteCarImagesOfCarCommand(id), cancellationToken);
        return V1Results.Ok(null);
    }

    private static FileUpload ToUpload(IFormFile file)
    {
        return file == null ? null : new FileUpload(file.FileName, file.Length, file.OpenReadStream);
    }
}
