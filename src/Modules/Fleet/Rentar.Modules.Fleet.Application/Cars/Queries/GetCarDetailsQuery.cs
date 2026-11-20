using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Every car with the names of its brand and color, or one page of them.</summary>
public sealed record GetCarDetailsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetCarDetailsQueryHandler(ICarDal carDal) : IQueryHandler<GetCarDetailsQuery, Page<CarDetailResponse>>
{
    public Task<Page<CarDetailResponse>> HandleAsync(GetCarDetailsQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsPageAsync(query.Page, cancellationToken);
    }
}
