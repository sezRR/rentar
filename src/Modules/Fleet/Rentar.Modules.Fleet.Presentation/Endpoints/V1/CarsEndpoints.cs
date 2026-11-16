using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Application.Cars.Commands;
using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Models;

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

    private static Ok<V1Response<List<Car>>> GetAll(IQueryHandler<GetCarsQuery, List<Car>> getCars)
    {
        return V1Results.Ok(getCars.Handle(new GetCarsQuery()), Messages.CarsListed);
    }

    private static Ok<V1Response<List<CarDetailResponse>>> GetCarDetails(IQueryHandler<GetCarDetailsQuery, List<CarDetailResponse>> getCarDetails)
    {
        return V1Results.Ok(getCarDetails.Handle(new GetCarDetailsQuery()), null);
    }

    private static Ok<V1Response<CarDetailResponse>> GetCarDetailsById(IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse> getCarDetailsById, int id)
    {
        return V1Results.Ok(getCarDetailsById.Handle(new GetCarDetailsByIdQuery(id)), null);
    }

    private static Ok<V1Response<List<CarDetailResponse>>> GetByBrandId(IQueryHandler<GetCarsByBrandQuery, List<CarDetailResponse>> getCarsByBrand, int brandId)
    {
        return V1Results.Ok(getCarsByBrand.Handle(new GetCarsByBrandQuery(brandId)), null);
    }

    private static Ok<V1Response<List<CarDetailResponse>>> GetByColorId(IQueryHandler<GetCarsByColorQuery, List<CarDetailResponse>> getCarsByColor, int colorId)
    {
        return V1Results.Ok(getCarsByColor.Handle(new GetCarsByColorQuery(colorId)), Messages.CarsListed);
    }

    private static Ok<V1Response<List<CarDetailResponse>>> GetCarsByBrandIdAndColorId(
        IQueryHandler<GetCarsByBrandAndColorQuery, List<CarDetailResponse>> getCarsByBrandAndColor,
        int brandId,
        int colorId)
    {
        return V1Results.Ok(getCarsByBrandAndColor.Handle(new GetCarsByBrandAndColorQuery(brandId, colorId)), null);
    }

    private static Ok<V1Response> Add(ICommandHandler<CreateCarCommand, Car> createCar, CreateCarCommand command)
    {
        createCar.Handle(command);
        return V1Results.Ok(Messages.CarAdded);
    }

    /// <summary>
    /// Like <c>add</c>, but answers the stored car.
    /// </summary>
    private static Ok<V1Response<Car>> AddReturnableData(ICommandHandler<CreateCarCommand, Car> createCar, CreateCarCommand command)
    {
        return V1Results.Ok(createCar.Handle(command), null);
    }

    private static Ok<V1Response> Update(ICommandHandler<UpdateCarCommand> updateCar, UpdateCarCommand command)
    {
        updateCar.Handle(command);
        return V1Results.Ok(Messages.CarUpdated);
    }

    private static Ok<V1Response> Delete(ICommandHandler<DeleteCarCommand> deleteCar, DeleteCarCommand command)
    {
        deleteCar.Handle(command);
        return V1Results.Ok(Messages.CarDeleted);
    }
}
