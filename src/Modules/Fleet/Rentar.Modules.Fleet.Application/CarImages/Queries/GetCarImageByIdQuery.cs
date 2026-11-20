using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>Answers the image, or <c>null</c> when there is none with that id.</summary>
public sealed record GetCarImageByIdQuery(int Id);

public sealed class GetCarImageByIdQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImageByIdQuery, CarImage>
{
    public Task<CarImage> HandleAsync(GetCarImageByIdQuery query, CancellationToken cancellationToken = default)
    {
        return carImageDal.GetAsync(c => c.Id == query.Id, cancellationToken);
    }
}
