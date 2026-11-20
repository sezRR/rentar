using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>The images of every car, or one page of them.</summary>
public sealed record GetCarImagesQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetCarImagesQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesQuery, Page<CarImage>>
{
    public Task<Page<CarImage>> HandleAsync(GetCarImagesQuery query, CancellationToken cancellationToken = default)
    {
        return carImageDal.GetPageAsync(query.Page, cancellationToken);
    }
}
