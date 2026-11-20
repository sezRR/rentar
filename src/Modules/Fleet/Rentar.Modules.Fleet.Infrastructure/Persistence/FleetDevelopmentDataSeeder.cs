using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Infrastructure.Persistence;

internal sealed class FleetDevelopmentDataSeeder(IDbContextFactory<FleetDbContext> contextFactory, TimeProvider timeProvider)
    : IDevelopmentDataSeeder
{
    public int Order => 20;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (key, brandName, colorName, description, year, price, rating) in new[]
        {
            ("fleet.corolla", "Toyota", "White", "Corolla 1.8 Hybrid", 2024, 1500m, 900),
            ("fleet.clio", "Renault", "Grey", "Clio 1.0 TCe", 2023, 1100m, 700),
            ("fleet.bmw", "BMW", "Black", "320i Sedan", 2024, 2800m, 1200),
            ("fleet.yaris", "Toyota", "Grey", "Yaris 1.5 Hybrid", 2023, 1300m, 800)
        })
        {
            var brand = await database.Brands.FirstOrDefaultAsync(brand => brand.BrandName == brandName, cancellationToken);
            if (brand is null)
            {
                brand = new Brand(brandName);
                database.Brands.Add(brand);
                await database.SaveChangesAsync(cancellationToken);
            }

            var color = await database.Colors.FirstOrDefaultAsync(color => color.ColorName == colorName, cancellationToken);
            if (color is null)
            {
                color = new Color(colorName);
                database.Colors.Add(color);
                await database.SaveChangesAsync(cancellationToken);
            }

            var car = await database.Cars.FirstOrDefaultAsync(
                car => car.BrandId == brand.Id && car.Description == description, cancellationToken);
            if (car is null)
            {
                car = new Car(brand.Id, color.Id, year, price, description, rating);
                database.Cars.Add(car);
                await database.SaveChangesAsync(cancellationToken);
                database.CarImages.Add(car.NewImage(IImageStorage.DefaultImageName, timeProvider.GetLocalNow().DateTime));
                await database.SaveChangesAsync(cancellationToken);
            }

            context.SetId(key, car.Id);
        }
    }
}
