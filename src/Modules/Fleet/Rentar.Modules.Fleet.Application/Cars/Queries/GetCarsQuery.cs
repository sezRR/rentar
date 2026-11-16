using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Aspects.Performance;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsQuery;

public sealed class GetCarsQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsQuery, List<Car>>
{
    [PerformanceAspect(5)]
    public Task<List<Car>> HandleAsync(GetCarsQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetAllAsync(cancellationToken);
    }
}
