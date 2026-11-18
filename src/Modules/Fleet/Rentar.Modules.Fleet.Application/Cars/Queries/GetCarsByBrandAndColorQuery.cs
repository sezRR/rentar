using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>The details of the cars of both the brand and the color, or one page of them.</summary>
public sealed record GetCarsByBrandAndColorQuery(int BrandId, int ColorId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCarsByBrandAndColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandAndColorQuery, Page<CarDetailResponse>>
{
    public Task<Page<CarDetailResponse>> HandleAsync(GetCarsByBrandAndColorQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsPageAsync(c => c.BrandId == query.BrandId && c.ColorId == query.ColorId, query.Page, cancellationToken);
    }
}
