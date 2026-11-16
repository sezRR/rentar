using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Every rental of the Fleet car <see cref="CarId"/>.</summary>
public sealed record GetRentalsByCarQuery(int CarId);

public sealed class GetRentalsByCarQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsByCarQuery, List<Rental>>
{
    public Task<List<Rental>> HandleAsync(GetRentalsByCarQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.GetAllAsync(r => r.CarId == query.CarId, cancellationToken);
    }
}
