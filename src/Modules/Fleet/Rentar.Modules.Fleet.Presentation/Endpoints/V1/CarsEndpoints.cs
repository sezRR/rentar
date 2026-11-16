using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Application.Cars.Commands;
using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V1;

public class CarsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Cars", "cars", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getcardetails", GetCarDetails);
        group.MapGet("/getcardetailsbyid", GetCarDetailsById);
        group.MapGet("/getcarsbybrandid", GetByBrandId);
        group.MapGet("/getcarsbycolorid", GetByColorId);
        group.MapGet("/getcarsbybrandidandcolorid", GetCarsByBrandIdAndColorId);
        group.MapPost("/add", Add);
        group.MapPost("/addreturnabledata", AddReturnableData);
        group.MapPost("/update", Update);
        group.MapPost("/delete", Delete);
    }

    private static async Task<Ok<V1Response<List<Car>>>> GetAll(IQueryHandler<GetCarsQuery, List<Car>> getCars, CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCars.HandleAsync(new GetCarsQuery(), cancellationToken), Messages.CarsListed);
    }

    private static async Task<Ok<V1Response<List<CarDetailResponse>>>> GetCarDetails(
        IQueryHandler<GetCarDetailsQuery, List<CarDetailResponse>> getCarDetails,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarDetails.HandleAsync(new GetCarDetailsQuery(), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<CarDetailResponse>>> GetCarDetailsById(
        IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById,
        int id,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarDetailsById.HandleAsync(new GetCarDetailsByIdQuery(id), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<List<CarDetailResponse>>>> GetByBrandId(
        IQueryHandler<GetCarsByBrandQuery, List<CarDetailResponse>> getCarsByBrand,
        int brandId,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarsByBrand.HandleAsync(new GetCarsByBrandQuery(brandId), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<List<CarDetailResponse>>>> GetByColorId(
        IQueryHandler<GetCarsByColorQuery, List<CarDetailResponse>> getCarsByColor,
        int colorId,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCarsByColor.HandleAsync(new GetCarsByColorQuery(colorId), cancellationToken), Messages.CarsListed);
    }

    private static async Task<Ok<V1Response<List<CarDetailResponse>>>> GetCarsByBrandIdAndColorId(
        IQueryHandler<GetCarsByBrandAndColorQuery, List<CarDetailResponse>> getCarsByBrandAndColor,
        int brandId,
        int colorId,
        CancellationToken cancellationToken)
    {
        var cars = await getCarsByBrandAndColor.HandleAsync(new GetCarsByBrandAndColorQuery(brandId, colorId), cancellationToken);
        return V1Results.Ok(cars, null);
    }

    private static async Task<Ok<V1Response>> Add(ICommandHandler<CreateCarCommand, Car> createCar, CreateCarCommand command, CancellationToken cancellationToken)
    {
        await createCar.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.CarAdded);
    }

    /// <summary>
    /// Like <c>add</c>, but answers the stored car.
    /// </summary>
    private static async Task<Ok<V1Response<Car>>> AddReturnableData(
        ICommandHandler<CreateCarCommand, Car> createCar,
        CreateCarCommand command,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await createCar.HandleAsync(command, cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Update(ICommandHandler<UpdateCarCommand> updateCar, UpdateCarCommand command, CancellationToken cancellationToken)
    {
        await updateCar.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.CarUpdated);
    }

    private static async Task<Ok<V1Response>> Delete(ICommandHandler<DeleteCarCommand> deleteCar, DeleteCarCommand command, CancellationToken cancellationToken)
    {
        await deleteCar.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.CarDeleted);
    }
}
