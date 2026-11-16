using System.Collections.Generic;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Every rental of the Fleet car <see cref="CarId"/>.</summary>
public sealed record GetRentalsByCarQuery(int CarId);

public sealed class GetRentalsByCarQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsByCarQuery, List<Rental>>
{
    public List<Rental> Handle(GetRentalsByCarQuery query)
    {
        return rentalDal.GetAll(r => r.CarId == query.CarId);
    }
}
