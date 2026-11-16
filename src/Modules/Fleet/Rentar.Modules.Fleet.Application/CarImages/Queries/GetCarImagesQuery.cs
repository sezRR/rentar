using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>The images of every car.</summary>
public sealed record GetCarImagesQuery;

public sealed class GetCarImagesQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesQuery, List<CarImage>>
{
    public Task<List<CarImage>> HandleAsync(GetCarImagesQuery query, CancellationToken cancellationToken = default)
    {
        return carImageDal.GetAllAsync(cancellationToken);
    }
}
