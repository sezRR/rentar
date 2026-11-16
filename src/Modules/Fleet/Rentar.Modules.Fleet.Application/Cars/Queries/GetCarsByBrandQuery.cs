using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Aspects.Caching;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByBrandQuery(int BrandId);

public sealed class GetCarsByBrandQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandQuery, List<CarDetailResponse>>
{
    [CacheAspect(duration: 10)]
    public Task<List<CarDetailResponse>> HandleAsync(GetCarsByBrandQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsAsync(c => c.BrandId == query.BrandId, cancellationToken);
    }
}
