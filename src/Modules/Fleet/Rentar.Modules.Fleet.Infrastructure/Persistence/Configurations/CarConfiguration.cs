using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Fleet.Domain;

namespace Rentar.Modules.Fleet.Infrastructure.Persistence.Configurations;

/// <summary>
/// A car belongs to one brand and one color (one-to-many each) and owns its images (one-to-many).
/// </summary>
internal sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        // Deleting a brand or color would otherwise take its cars, and their bookings in Rentals, with it.
        builder.HasOne(car => car.Brand)
            .WithMany(brand => brand.Cars)
            .HasForeignKey(car => car.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(car => car.Color)
            .WithMany(color => color.Cars)
            .HasForeignKey(car => car.ColorId)
            .OnDelete(DeleteBehavior.Restrict);

        // An image means nothing without its car. The database removes the rows; DeleteCarCommand removes the files.
        builder.HasMany(car => car.Images)
            .WithOne(image => image.Car)
            .HasForeignKey(image => image.CarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
