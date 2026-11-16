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
using System.Threading;
using System.Threading.Tasks;

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
    private static async Task<Ok<List<Rental>>> GetAll(
        IQueryHandler<GetRentalsQuery, List<Rental>> getRentals,
        IQueryHandler<GetRentalsByCarQuery, List<Rental>> getRentalsByCar,
        int? carId,
        CancellationToken cancellationToken)
    {
        var rentals = carId is { } car
            ? await getRentalsByCar.HandleAsync(new GetRentalsByCarQuery(car), cancellationToken)
            : await getRentals.HandleAsync(new GetRentalsQuery(), cancellationToken);

        return TypedResults.Ok(rentals);
    }

    private static async Task<Results<Ok<Rental>, ProblemHttpResult>> GetById(
        IQueryHandler<GetRentalByIdQuery, Rental> getRentalById,
        int id,
        CancellationToken cancellationToken)
    {
        var rental = await getRentalById.HandleAsync(new GetRentalByIdQuery(id), cancellationToken);
        return rental is null ? RestResults.NotFound("Rental", id) : TypedResults.Ok(rental);
    }

    /// <summary>
    /// Rentals together with their car, customer and user; a rental whose car, customer or user is gone is left out.
    /// Closed during the 22:00 maintenance hour.
    /// </summary>
    private static async Task<Ok<List<RentalDetailResponse>>> GetDetails(
        IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>> getRentalDetails,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await getRentalDetails.HandleAsync(new GetRentalDetailsQuery(), cancellationToken));
    }

    private static async Task<Ok<RentalAvailabilityResponse>> GetAvailability(
        IQueryHandler<CheckCarAvailabilityQuery, bool> checkCarAvailability,
        int carId,
        DateTime rentDate,
        DateTime returnDate,
        CancellationToken cancellationToken)
    {
        var available = await checkCarAvailability.HandleAsync(new CheckCarAvailabilityQuery(carId, rentDate, returnDate), cancellationToken);
        return TypedResults.Ok(new RentalAvailabilityResponse(carId, rentDate, returnDate, available));
    }

    private static async Task<Created<Rental>> Create(
        ICommandHandler<CreateRentalCommand, Rental> createRental,
        HttpRequest request,
        CreateRentalCommand command,
        CancellationToken cancellationToken)
    {
        var rental = await createRental.HandleAsync(command, cancellationToken);
        return RestResults.Created(request, rental.Id, rental);
    }
}
