using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

public sealed record GetCarImagesByCarQuery(int CarId);

public sealed class GetCarImagesByCarQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesByCarQuery, List<CarImage>>
{
    public List<CarImage> Handle(GetCarImagesByCarQuery query)
    {
        return carImageDal.GetAll(c => c.CarId == query.CarId);
    }
}
