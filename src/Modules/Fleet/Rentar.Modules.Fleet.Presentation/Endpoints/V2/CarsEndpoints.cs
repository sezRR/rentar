#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.Cars.Commands;
using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

public class CarsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Cars", "cars", 2.0);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Lists one page of cars, optionally only those of a brand, a color, or both. Each filter is its own use case.
    /// </summary>
    private static async Task<Ok<PagedResponse<CarResponse>>> GetAll(
        IQueryHandler<GetCarDetailsQuery, Page<CarDetailResponse>> getCarDetails,
        IQueryHandler<GetCarsByBrandQuery, Page<CarDetailResponse>> getCarsByBrand,
        IQueryHandler<GetCarsByColorQuery, Page<CarDetailResponse>> getCarsByColor,
        IQueryHandler<GetCarsByBrandAndColorQuery, Page<CarDetailResponse>> getCarsByBrandAndColor,
        int? brandId,
        int? colorId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        var page = paging.ToRequest();
        var cars = (brandId, colorId) switch
        {
            ({ } brand, { } color) => await getCarsByBrandAndColor.HandleAsync(new GetCarsByBrandAndColorQuery(brand, color, page), cancellationToken),
            ({ } brand, null) => await getCarsByBrand.HandleAsync(new GetCarsByBrandQuery(brand, page), cancellationToken),
            (null, { } color) => await getCarsByColor.HandleAsync(new GetCarsByColorQuery(color, page), cancellationToken),
            _ => await getCarDetails.HandleAsync(new GetCarDetailsQuery(page), cancellationToken)
        };

        return RestResults.Paged(cars.Map(CarResponse.From));
    }

    private static async Task<Results<Ok<CarResponse>, ProblemHttpResult>> GetById(
        IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById,
        int id,
        CancellationToken cancellationToken)
    {
        var car = await getCarDetailsById.HandleAsync(new GetCarDetailsByIdQuery(id), cancellationToken);
        return car is null ? RestResults.NotFound("Car", id) : TypedResults.Ok(CarResponse.From(car));
    }

    /// <summary>
    /// Creates a car; a brand or color that is not stored answers 400.
    /// </summary>
    private static async Task<Created<CarResponse>> Create(
        ICommandHandler<CreateCarCommand, Car> createCar,
        IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById,
        HttpRequest request,
        CarRequest body,
        CancellationToken cancellationToken)
    {
        var car = await createCar.HandleAsync(
            new CreateCarCommand(body.BrandId, body.ColorId, body.ModelYear, body.DailyPrice, body.Description, body.MinimumFindeksRating),
            cancellationToken);

        var created = await getCarDetailsById.HandleAsync(new GetCarDetailsByIdQuery(car.Id), cancellationToken);
        return RestResults.Created(request, car.Id, CarResponse.From(created));
    }

    /// <summary>
    /// Replaces the car; a brand or color that is not stored answers 400.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Update(
        IQueryHandler<GetCarByIdQuery, Car> getCarById,
        ICommandHandler<UpdateCarCommand> updateCar,
        int id,
        CarRequest body,
        CancellationToken cancellationToken)
    {
        if (await getCarById.HandleAsync(new GetCarByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Car", id);
        }

        await updateCar.HandleAsync(
            new UpdateCarCommand(id, body.BrandId, body.ColorId, body.ModelYear, body.DailyPrice, body.Description, body.MinimumFindeksRating),
            cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes the car together with its images and their files; a car that has rentals answers 409.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
        IQueryHandler<GetCarByIdQuery, Car> getCarById,
        ICommandHandler<DeleteCarCommand> deleteCar,
        int id,
        CancellationToken cancellationToken)
    {
        if (await getCarById.HandleAsync(new GetCarByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Car", id);
        }

        await deleteCar.HandleAsync(new DeleteCarCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
