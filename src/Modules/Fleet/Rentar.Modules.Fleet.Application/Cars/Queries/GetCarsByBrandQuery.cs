using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Aspects.Caching;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByBrandQuery(int BrandId);

public sealed class GetCarsByBrandQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandQuery, List<CarDetailResponse>>
{
    [CacheAspect(duration: 10)]
    public List<CarDetailResponse> Handle(GetCarsByBrandQuery query)
    {
        return carDal.GetCarDetails(c => c.BrandId == query.BrandId);
    }
}
