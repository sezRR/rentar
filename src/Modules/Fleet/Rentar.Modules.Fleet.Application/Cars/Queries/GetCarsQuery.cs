using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Aspects.Performance;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsQuery;

public sealed class GetCarsQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsQuery, List<Car>>
{
    [PerformanceAspect(5)]
    public List<Car> Handle(GetCarsQuery query)
    {
        return carDal.GetAll();
    }
}
