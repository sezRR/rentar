using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Common.Interfaces;

public interface ICarDal : IEntityRepository<Car>
{
    Task<List<CarDetailResponse>> GetCarDetailsAsync(CancellationToken cancellationToken = default);
    Task<List<CarDetailResponse>> GetCarDetailsAsync(Expression<Func<Car, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>The car's details, or <c>null</c> when there is no car with that id.</summary>
    Task<CarDetailResponse> GetCarDetailsByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>The car with its <see cref="Car.Images"/>, or <c>null</c> when there is no car with that id.</summary>
    Task<Car> GetWithImagesAsync(int id, CancellationToken cancellationToken = default);
}
