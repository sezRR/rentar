using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

public sealed record GetCarsByBrandAndColorQuery(int BrandId, int ColorId);

public sealed class GetCarsByBrandAndColorQueryHandler(ICarDal carDal) : IQueryHandler<GetCarsByBrandAndColorQuery, List<CarDetailResponse>>
{
    public Task<List<CarDetailResponse>> HandleAsync(GetCarsByBrandAndColorQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsAsync(c => c.BrandId == query.BrandId && c.ColorId == query.ColorId, cancellationToken);
    }
}
