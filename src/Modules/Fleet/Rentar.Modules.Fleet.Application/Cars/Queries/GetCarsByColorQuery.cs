using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByColorQuery(int ColorId);

public sealed class GetCarsByColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByColorQuery, List<CarDetailResponse>>
{
    public List<CarDetailResponse> Handle(GetCarsByColorQuery query)
    {
        return carDal.GetCarDetails(c => c.ColorId == query.ColorId);
    }
}
