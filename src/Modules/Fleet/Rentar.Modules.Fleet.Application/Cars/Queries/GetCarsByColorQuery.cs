using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>The details of the color's cars, or one page of them.</summary>
public sealed record GetCarsByColorQuery(int ColorId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCarsByColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByColorQuery, Page<CarDetailResponse>>
{
    public Task<Page<CarDetailResponse>> HandleAsync(GetCarsByColorQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsPageAsync(c => c.ColorId == query.ColorId, query.Page, cancellationToken);
    }
}
