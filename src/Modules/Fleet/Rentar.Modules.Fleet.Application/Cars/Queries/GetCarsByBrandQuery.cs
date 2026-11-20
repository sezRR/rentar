using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Aspects.Caching;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>The details of the brand's cars, or one page of them.</summary>
public sealed record GetCarsByBrandQuery(int BrandId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCarsByBrandQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandQuery, Page<CarDetailResponse>>
{
    [CacheAspect(duration: 10)]
    public Task<Page<CarDetailResponse>> HandleAsync(GetCarsByBrandQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsPageAsync(c => c.BrandId == query.BrandId, query.Page, cancellationToken);
    }
}
