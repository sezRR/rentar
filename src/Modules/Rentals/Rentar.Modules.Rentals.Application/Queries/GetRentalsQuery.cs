using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

public sealed record GetRentalsQuery;

public sealed class GetRentalsQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsQuery, List<Rental>>
{
    public Task<List<Rental>> HandleAsync(GetRentalsQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.GetAllAsync(cancellationToken);
    }
}
