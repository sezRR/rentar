using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application;

public class FleetModuleApi(ICarDal carDal) : IFleetModuleApi
{
    public async Task<IReadOnlyList<CarSummary>> GetCarsAsync(IReadOnlyCollection<int> carIds, CancellationToken cancellationToken = default)
    {
        if (carIds.Count == 0)
        {
            return [];
        }

        var ids = carIds.ToArray();
        var cars = await carDal.GetCarDetailsAsync(c => ids.Contains(c.Id), cancellationToken);
        return cars.ConvertAll(c => new CarSummary(
            c.CarId, c.BrandId, c.BrandName, c.ColorId, c.ColorName, c.ModelYear, c.DailyPrice, c.Description));
    }
}
