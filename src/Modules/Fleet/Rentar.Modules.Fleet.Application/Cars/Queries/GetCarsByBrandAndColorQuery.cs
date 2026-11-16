using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByBrandAndColorQuery(int BrandId, int ColorId);

public sealed class GetCarsByBrandAndColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandAndColorQuery, List<CarDetailResponse>>
{
    public List<CarDetailResponse> Handle(GetCarsByBrandAndColorQuery query)
    {
        return carDal.GetCarDetails(c => c.BrandId == query.BrandId && c.ColorId == query.ColorId);
    }
}
