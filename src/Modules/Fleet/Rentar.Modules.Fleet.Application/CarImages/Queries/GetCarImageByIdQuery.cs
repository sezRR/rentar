using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>Answers the image, or <c>null</c> when there is none with that id.</summary>
public sealed record GetCarImageByIdQuery(int Id);

public sealed class GetCarImageByIdQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImageByIdQuery, CarImage>
{
    public CarImage Handle(GetCarImageByIdQuery query)
    {
        return carImageDal.Get(c => c.Id == query.Id);
    }
}
