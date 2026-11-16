#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Collections.Generic;

namespace Rentar.Modules.Rentals.Presentation.Endpoints.V2;

public class RentalsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Rentals", "rentals", 2.0);

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapGet("/details", GetDetails)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/availability", GetAvailability);
        group.MapPost("/", Create)
            .ProducesValidationProblem();
    }

    /// <summary>
    /// Lists rentals, optionally only those of a car.
    /// </summary>
    private static Ok<List<Rental>> GetAll(
        IQueryHandler<GetRentalsQuery, List<Rental>> getRentals,
        IQueryHandler<GetRentalsByCarQuery, List<Rental>> getRentalsByCar,
        int? carId)
    {
        var rentals = carId is { } car
            ? getRentalsByCar.Handle(new GetRentalsByCarQuery(car))
            : getRentals.Handle(new GetRentalsQuery());

        return TypedResults.Ok(rentals);
    }

    private static Results<Ok<Rental>, ProblemHttpResult> GetById(IQueryHandler<GetRentalByIdQuery, Rental> getRentalById, int id)
    {
        var rental = getRentalById.Handle(new GetRentalByIdQuery(id));
        return rental is null ? RestResults.NotFound("Rental", id) : TypedResults.Ok(rental);
    }

    /// <summary>
    /// Rentals together with their car, customer and user; a rental whose car, customer or user is gone is left out.
    /// Closed during the 22:00 maintenance hour.
    /// </summary>
    private static Ok<List<RentalDetailResponse>> GetDetails(IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>> getRentalDetails)
    {
        return TypedResults.Ok(getRentalDetails.Handle(new GetRentalDetailsQuery()));
    }

    private static Ok<RentalAvailabilityResponse> GetAvailability(
        IQueryHandler<CheckCarAvailabilityQuery, bool> checkCarAvailability,
        int carId,
        DateTime rentDate,
        DateTime returnDate)
    {
        var available = checkCarAvailability.Handle(new CheckCarAvailabilityQuery(carId, rentDate, returnDate));
        return TypedResults.Ok(new RentalAvailabilityResponse(carId, rentDate, returnDate, available));
    }

    private static Created<Rental> Create(ICommandHandler<CreateRentalCommand, Rental> createRental, HttpRequest request, CreateRentalCommand command)
    {
        var rental = createRental.Handle(command);
        return RestResults.Created(request, rental.Id, rental);
    }
}
