using System.Collections.Generic;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Rentals.Application.Queries;

public sealed record GetRentalsQuery;

public sealed class GetRentalsQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsQuery, List<Rental>>
{
    public List<Rental> Handle(GetRentalsQuery query)
    {
        return rentalDal.GetAll();
    }
}
