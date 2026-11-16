#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.Cars.Commands;
using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Models;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

public class CarsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Cars", "cars", 2.0);

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem();
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Lists cars, optionally only those of a brand, a color, or both. Each filter is its own use case.
    /// </summary>
    private static Ok<List<CarResponse>> GetAll(
        IQueryHandler<GetCarDetailsQuery, List<CarDetailResponse>> getCarDetails,
        IQueryHandler<GetCarsByBrandQuery, List<CarDetailResponse>> getCarsByBrand,
        IQueryHandler<GetCarsByColorQuery, List<CarDetailResponse>> getCarsByColor,
        IQueryHandler<GetCarsByBrandAndColorQuery, List<CarDetailResponse>> getCarsByBrandAndColor,
        int? brandId,
        int? colorId)
    {
        var cars = (brandId, colorId) switch
        {
            ({ } brand, { } color) => getCarsByBrandAndColor.Handle(new GetCarsByBrandAndColorQuery(brand, color)),
            ({ } brand, null) => getCarsByBrand.Handle(new GetCarsByBrandQuery(brand)),
            (null, { } color) => getCarsByColor.Handle(new GetCarsByColorQuery(color)),
            _ => getCarDetails.Handle(new GetCarDetailsQuery())
        };

        return TypedResults.Ok(cars.ConvertAll(CarResponse.From));
    }

    private static Results<Ok<CarResponse>, ProblemHttpResult> GetById(
        IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById,
        int id)
    {
        var car = getCarDetailsById.Handle(new GetCarDetailsByIdQuery(id));
        return car is null ? RestResults.NotFound("Car", id) : TypedResults.Ok(CarResponse.From(car));
    }

    private static Created<CarResponse> Create(
        ICommandHandler<CreateCarCommand, Car> createCar,
        IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById,
        HttpRequest request,
        CarRequest body)
    {
        var car = createCar.Handle(new CreateCarCommand(
            body.BrandId, body.ColorId, body.ModelYear, body.DailyPrice, body.Description, body.MinimumFindeksRating));

        var created = getCarDetailsById.Handle(new GetCarDetailsByIdQuery(car.Id));
        return RestResults.Created(request, car.Id, created is null ? CarResponse.From(car) : CarResponse.From(created));
    }

    private static Results<NoContent, ProblemHttpResult> Update(
        IQueryHandler<GetCarByIdQuery, Car> getCarById,
        ICommandHandler<UpdateCarCommand> updateCar,
        int id,
        CarRequest body)
    {
        if (getCarById.Handle(new GetCarByIdQuery(id)) is null)
        {
            return RestResults.NotFound("Car", id);
        }

        updateCar.Handle(new UpdateCarCommand(
            id, body.BrandId, body.ColorId, body.ModelYear, body.DailyPrice, body.Description, body.MinimumFindeksRating));
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes the car together with its images and their files.
    /// </summary>
    private static Results<NoContent, ProblemHttpResult> Delete(
        IQueryHandler<GetCarByIdQuery, Car> getCarById,
        ICommandHandler<DeleteCarCommand> deleteCar,
        int id)
    {
        if (getCarById.Handle(new GetCarByIdQuery(id)) is null)
        {
            return RestResults.NotFound("Car", id);
        }

        deleteCar.Handle(new DeleteCarCommand(id));
        return TypedResults.NoContent();
    }
}
