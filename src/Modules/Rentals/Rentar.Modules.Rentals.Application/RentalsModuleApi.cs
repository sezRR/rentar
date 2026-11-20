using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Contracts;
using Rentar.Shared.Application.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application;

public class RentalsModuleApi(
    IQueryHandler<CheckCarAvailabilityQuery, bool> checkCarAvailability,
    IQueryHandler<CheckCarHasRentalsQuery, bool> checkCarHasRentals) : IRentalsModuleApi
{
    public Task<bool> IsCarAvailableAsync(int carId, DateTime rentDate, DateTime returnDate, CancellationToken cancellationToken = default)
    {
        return checkCarAvailability.HandleAsync(new CheckCarAvailabilityQuery(carId, rentDate, returnDate), cancellationToken);
    }

    public Task<bool> HasRentalsAsync(int carId, CancellationToken cancellationToken = default)
    {
        return checkCarHasRentals.HandleAsync(new CheckCarHasRentalsQuery(carId), cancellationToken);
    }
}
