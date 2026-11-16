using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Rentals.Domain;

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Configurations;

internal sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        // The car lives in the Fleet schema, so there is no foreign key to index it; rentals are looked up by car.
        builder.HasIndex(rental => rental.CarId);
    }
}
