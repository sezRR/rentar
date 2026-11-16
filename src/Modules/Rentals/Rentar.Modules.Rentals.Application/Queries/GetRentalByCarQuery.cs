using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// The one rental of the Fleet car <see cref="CarId"/>, or <c>null</c>, as v1 asks for it. It fails when the car has
/// more than one rental; <see cref="GetRentalsByCarQuery"/> lists them all.
/// </summary>
public sealed record GetRentalByCarQuery(int CarId);

public sealed class GetRentalByCarQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalByCarQuery, Rental>
{
    public Rental Handle(GetRentalByCarQuery query)
    {
        return rentalDal.Get(r => r.CarId == query.CarId);
    }
}
