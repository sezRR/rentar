using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

public sealed record GetCarImagesByCarQuery(int CarId);

public sealed class GetCarImagesByCarQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesByCarQuery, List<CarImage>>
{
    public Task<List<CarImage>> HandleAsync(GetCarImagesByCarQuery query, CancellationToken cancellationToken = default)
    {
        return carImageDal.GetAllAsync(c => c.CarId == query.CarId, cancellationToken);
    }
}
