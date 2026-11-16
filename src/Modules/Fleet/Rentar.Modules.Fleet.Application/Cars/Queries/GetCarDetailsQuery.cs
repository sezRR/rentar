using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Every car with the names of its brand and color.</summary>
public sealed record GetCarDetailsQuery;

public sealed class GetCarDetailsQueryHandler(ICarDal carDal) : IQueryHandler<GetCarDetailsQuery, List<CarDetailResponse>>
{
    public Task<List<CarDetailResponse>> HandleAsync(GetCarDetailsQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsAsync(cancellationToken);
    }
}
