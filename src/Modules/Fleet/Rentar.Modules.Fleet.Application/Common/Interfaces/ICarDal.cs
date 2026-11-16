using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;

namespace Rentar.Modules.Fleet.Application.Common.Interfaces;

public interface ICarDal : IEntityRepository<Car>
{
    List<CarDetailResponse> GetCarDetails(Expression<Func<Car, bool>> filter = null);
    CarDetailResponse GetCarDetailsById(Expression<Func<Car, bool>> filter);
}
