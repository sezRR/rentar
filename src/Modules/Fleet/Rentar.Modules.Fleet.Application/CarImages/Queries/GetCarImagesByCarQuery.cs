using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>The images of the car <see cref="CarId"/>, or one page of them.</summary>
public sealed record GetCarImagesByCarQuery(int CarId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCarImagesByCarQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesByCarQuery, Page<CarImage>>
{
    public Task<Page<CarImage>> HandleAsync(GetCarImagesByCarQuery query, CancellationToken cancellationToken = default)
    {
        return carImageDal.GetPageAsync(c => c.CarId == query.CarId, query.Page, cancellationToken);
    }
}
