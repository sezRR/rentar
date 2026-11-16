using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Answers the car's details, or <c>null</c> when there is no such car.</summary>
public sealed record GetCarDetailsByIdQuery(int Id);

public sealed class GetCarDetailsByIdQueryHandler(ICarDal carDal) : IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse>
{
    public Task<CarDetailResponse> HandleAsync(GetCarDetailsByIdQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetCarDetailsByIdAsync(query.Id, cancellationToken);
    }
}
