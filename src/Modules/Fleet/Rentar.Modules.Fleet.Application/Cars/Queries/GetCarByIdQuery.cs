using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Answers the car, or <c>null</c> when there is none with that id.</summary>
public sealed record GetCarByIdQuery(int Id);

public sealed class GetCarByIdQueryHandler(ICarDal carDal) : IQueryHandler<GetCarByIdQuery, Car>
{
    public Task<Car> HandleAsync(GetCarByIdQuery query, CancellationToken cancellationToken = default)
    {
        return carDal.GetAsync(c => c.Id == query.Id, cancellationToken);
    }
}
