#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Security;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Presentation.Endpoints.V2;

public class RentalsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Rentals", "rentals", 2.0);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapGet("/details", GetDetails)
            .ProducesValidationProblem();
        group.MapGet("/availability", GetAvailability);
        group.MapPost("/", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // A company's rentals are addressed under it, and only its employees with the permission may see them.
        builder.MapVersionedGroup("Rentals", "companies/{companyId:int}/rentals", 2.0)
            .RequireAuthorization()
            .MapGet("/", GetByCompany)
            .ProducesValidationProblem()
            // Authentication refuses a missing or invalid token with an empty 401, not a problem.
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// One page of the rentals booked by the company's customers; needs <c>rentals.read</c> in the company. A company
    /// the caller does not work at answers 404.
    /// </summary>
    private static async Task<Ok<PagedResponse<Rental>>> GetByCompany(
        IQueryHandler<GetCompanyRentalsQuery, Page<Rental>> getCompanyRentals,
        ICurrentUser currentUser,
        int companyId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getCompanyRentals.HandleAsync(
            new GetCompanyRentalsQuery(currentUser.RequireUserId(), companyId, paging.ToRequest()),
            cancellationToken));
    }

    /// <summary>
    /// Lists one page of rentals, optionally only those of a car, cancelled ones included; <c>status</c> tells them apart.
    /// </summary>
    private static async Task<Ok<PagedResponse<Rental>>> GetAll(
        IQueryHandler<GetRentalsQuery, Page<Rental>> getRentals,
        IQueryHandler<GetRentalsByCarQuery, Page<Rental>> getRentalsByCar,
        int? carId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        var rentals = carId is { } car
            ? await getRentalsByCar.HandleAsync(new GetRentalsByCarQuery(car, paging.ToRequest()), cancellationToken)
            : await getRentals.HandleAsync(new GetRentalsQuery(paging.ToRequest()), cancellationToken);

        return RestResults.Paged(rentals);
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
    /// One page of rentals together with their car, customer and user; a cancelled rental, and one whose car, customer
    /// or user is gone, is left out.
    /// </summary>
    private static async Task<Ok<PagedResponse<RentalDetailResponse>>> GetDetails(
        IQueryHandler<GetRentalDetailsQuery, Page<RentalDetailResponse>> getRentalDetails,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getRentalDetails.HandleAsync(new GetRentalDetailsQuery(paging.ToRequest()), cancellationToken));
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

    /// <summary>
    /// Books the rental; answers 201 with it <c>Pending</c>. Its payment is taken right after, and turns it
    /// <c>Confirmed</c>, or <c>Cancelled</c> when declined: a cancelled rental is kept, but holds no car or customer.
    /// An overlapping active booking of either the car or the customer answers 409.
    /// </summary>
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
