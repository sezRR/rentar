using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Rentals.Application;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using Rentar.Modules.Rentals.Application.Common.Models;

namespace Rentar.Modules.Rentals.Presentation.Endpoints.V1;

public class RentalsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Rentals", "rentals", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getrentalbycarid", GetRentalByCarId);
        group.MapGet("/getrentaldetails", GetRentalDetails);
        group.MapPost("/isrentable", IsRentable);
        group.MapPost("/add", Add);
    }

    private static Ok<V1Response<List<Rental>>> GetAll(IQueryHandler<GetRentalsQuery, List<Rental>> getRentals)
    {
        return V1Results.Ok(getRentals.Handle(new GetRentalsQuery()), Messages.RentalsListed);
    }

    private static Ok<V1Response<Rental>> GetRentalByCarId(IQueryHandler<GetRentalByCarQuery, Rental> getRentalByCar, int carId)
    {
        return V1Results.Ok(getRentalByCar.Handle(new GetRentalByCarQuery(carId)), null);
    }

    private static Ok<V1Response<List<RentalDetailResponse>>> GetRentalDetails(
        IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>> getRentalDetails)
    {
        return V1Results.Ok(getRentalDetails.Handle(new GetRentalDetailsQuery()), null);
    }

    private static Results<Ok<V1Response>, BadRequest<V1Response>> IsRentable(
        IQueryHandler<CheckCarAvailabilityQuery, bool> checkCarAvailability,
        CheckCarAvailabilityQuery query)
    {
        return checkCarAvailability.Handle(query)
            ? V1Results.Ok(null)
            : V1Results.Fail(Messages.CarNotAvailable);
    }

    private static Ok<V1Response> Add(ICommandHandler<CreateRentalCommand, Rental> createRental, CreateRentalCommand command)
    {
        createRental.Handle(command);
        return V1Results.Ok(Messages.RentalAdded);
    }
}
