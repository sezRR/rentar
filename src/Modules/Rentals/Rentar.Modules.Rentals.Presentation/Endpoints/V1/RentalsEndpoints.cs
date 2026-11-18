using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Rentals.Application;
using Rentar.Modules.Rentals.Application.Commands;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>The rentals that hold their car; v1 knows no statuses, so cancelled ones are left out.</summary>
    private static async Task<Ok<V1Response<List<RentalResponse>>>> GetAll(
        IQueryHandler<GetRentalsQuery, List<Rental>> getRentals,
        CancellationToken cancellationToken)
    {
        var rentals = await getRentals.HandleAsync(new GetRentalsQuery(), cancellationToken);
        return V1Results.Ok(
            rentals.Where(r => r.Status != RentalStatus.Cancelled).Select(RentalResponse.From).ToList(),
            Messages.RentalsListed);
    }

    private static async Task<Ok<V1Response<RentalResponse>>> GetRentalByCarId(
        IQueryHandler<GetRentalByCarQuery, Rental> getRentalByCar,
        int carId,
        CancellationToken cancellationToken)
    {
        var rental = await getRentalByCar.HandleAsync(new GetRentalByCarQuery(carId), cancellationToken);
        return V1Results.Ok(rental is null ? null : RentalResponse.From(rental), null);
    }

    private static async Task<Ok<V1Response<List<RentalDetailResponse>>>> GetRentalDetails(
        IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>> getRentalDetails,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getRentalDetails.HandleAsync(new GetRentalDetailsQuery(), cancellationToken), null);
    }

    private static async Task<Results<Ok<V1Response>, BadRequest<V1Response>>> IsRentable(
        IQueryHandler<CheckCarAvailabilityQuery, bool> checkCarAvailability,
        CheckCarAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        return await checkCarAvailability.HandleAsync(query, cancellationToken)
            ? V1Results.Ok(null)
            : V1Results.Fail(Messages.CarNotAvailable);
    }

    /// <summary>
    /// Books the rental, whose payment is taken right after. A declined payment cancels the rental, which v1 then no
    /// longer lists. An overlapping active booking of the car or customer answers 400 in the v1 envelope.
    /// </summary>
    private static async Task<Ok<V1Response>> Add(
        ICommandHandler<CreateRentalCommand, Rental> createRental,
        CreateRentalCommand command,
        CancellationToken cancellationToken)
    {
        await createRental.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.RentalAdded);
    }
}
