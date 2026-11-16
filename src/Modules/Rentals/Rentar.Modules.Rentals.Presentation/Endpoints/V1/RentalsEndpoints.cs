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

    private static async Task<Ok<V1Response<List<Rental>>>> GetAll(IQueryHandler<GetRentalsQuery, List<Rental>> getRentals, CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getRentals.HandleAsync(new GetRentalsQuery(), cancellationToken), Messages.RentalsListed);
    }

    private static async Task<Ok<V1Response<Rental>>> GetRentalByCarId(
        IQueryHandler<GetRentalByCarQuery, Rental> getRentalByCar,
        int carId,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getRentalByCar.HandleAsync(new GetRentalByCarQuery(carId), cancellationToken), null);
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

    private static async Task<Ok<V1Response>> Add(ICommandHandler<CreateRentalCommand, Rental> createRental, CreateRentalCommand command, CancellationToken cancellationToken)
    {
        await createRental.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.RentalAdded);
    }
}
