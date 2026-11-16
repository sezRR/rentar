using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Every car with the names of its brand and color; a car whose brand or color is not stored is left out.</summary>
public sealed record GetCarDetailsQuery;

public sealed class GetCarDetailsQueryHandler(ICarDal carDal) : IQueryHandler<GetCarDetailsQuery, List<CarDetailResponse>>
{
    public List<CarDetailResponse> Handle(GetCarDetailsQuery query)
    {
        return carDal.GetCarDetails();
    }
}
