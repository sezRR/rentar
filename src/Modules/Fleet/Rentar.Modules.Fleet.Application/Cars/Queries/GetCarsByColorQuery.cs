using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByColorQuery(int ColorId);

public sealed class GetCarsByColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByColorQuery, List<CarDetailResponse>>
{
    public Task<List<CarDetailResponse>> HandleAsync(GetCarsByColorQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsAsync(c => c.ColorId == query.ColorId, cancellationToken);
    }
}
