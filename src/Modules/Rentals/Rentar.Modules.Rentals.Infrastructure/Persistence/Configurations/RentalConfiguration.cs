using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Rentals.Domain;

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Configurations;

internal sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        // The car and the customer live in other schemas, so there are no foreign keys to index them; rentals are
        // looked up by car, and by customer for a company's rentals.
        builder.HasIndex(rental => rental.CarId);
        builder.HasIndex(rental => rental.CustomerId);

        // PreventOverlappingRentals also installs partial GiST exclusion constraints for both ids and their periods.
        // PostgreSQL exclusions are maintained in that migration's SQL, as EF does not model them.

        // By name, so the column reads on its own and reordering the enum never changes what a row means.
        builder.Property(rental => rental.Status)
            .HasConversion<string>()
            .HasMaxLength(16);
    }
}
