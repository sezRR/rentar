using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Whether any rental, past or upcoming, is of the Fleet car <see cref="CarId"/>.</summary>
public sealed record CheckCarHasRentalsQuery(int CarId);

public sealed class CheckCarHasRentalsQueryHandler(IRentalDal rentalDal) : IQueryHandler<CheckCarHasRentalsQuery, bool>
{
    public Task<bool> HandleAsync(CheckCarHasRentalsQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.AnyAsync(r => r.CarId == query.CarId, cancellationToken);
    }
}
