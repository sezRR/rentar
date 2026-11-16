using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Application.Common.Models;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Infrastructure.Persistence;

public class EfCarDal(IDbContextFactory<FleetDbContext> contextFactory) : EfEntityRepositoryBase<Car, FleetDbContext>(contextFactory), ICarDal
{
    public async Task<List<CarDetailResponse>> GetCarDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await ToDetails(context.Cars).ToListAsync(cancellationToken);
    }

    public async Task<List<CarDetailResponse>> GetCarDetailsAsync(Expression<Func<Car, bool>> filter, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await ToDetails(context.Cars.Where(filter)).ToListAsync(cancellationToken);
    }

    public async Task<CarDetailResponse> GetCarDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await ToDetails(context.Cars.Where(car => car.Id == id)).SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Follows the navigations, which EF turns into joins: the required brand and color, and the first image uploaded.
    /// Only the columns the response needs are read.
    /// </summary>
    private static IQueryable<CarDetailResponse> ToDetails(IQueryable<Car> cars)
    {
        return cars.Select(car => new CarDetailResponse
        {
            Id = car.Id,
            CarId = car.Id,
            BrandId = car.BrandId,
            BrandName = car.Brand.BrandName,
            ColorId = car.ColorId,
            ColorName = car.Color.ColorName,
            Description = car.Description,
            ModelYear = car.ModelYear,
            DailyPrice = car.DailyPrice,
            MinimumFindeksRating = car.MinimumFindeksRating,
            ImagePath = car.Images.OrderBy(image => image.Id).Select(image => image.ImagePath).FirstOrDefault()
        });
    }
}
