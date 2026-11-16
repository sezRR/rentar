using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Cars.Queries;

/// <summary>Answers the car's details, or <c>null</c> when there is no such car or its brand or color is not stored.</summary>
public sealed record GetCarDetailsByIdQuery(int Id);

public sealed class GetCarDetailsByIdQueryHandler(ICarDal carDal) : IQueryHandler<GetCarDetailsByIdQuery, CarDetailResponse>
{
    public CarDetailResponse Handle(GetCarDetailsByIdQuery query)
    {
        return carDal.GetCarDetailsById(c => c.Id == query.Id);
    }
}
