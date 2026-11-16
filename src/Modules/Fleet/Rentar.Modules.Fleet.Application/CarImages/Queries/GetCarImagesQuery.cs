using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.CarImages.Queries;

/// <summary>The images of every car.</summary>
public sealed record GetCarImagesQuery;

public sealed class GetCarImagesQueryHandler(ICarImageDal carImageDal) : IQueryHandler<GetCarImagesQuery, List<CarImage>>
{
    public List<CarImage> Handle(GetCarImagesQuery query)
    {
        return carImageDal.GetAll();
    }
}
