using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Findeks.Domain;

namespace Rentar.Modules.Findeks.Infrastructure.Persistence.Configurations;

internal sealed class CustomerScoreConfiguration : IEntityTypeConfiguration<CustomerScore>
{
    public void Configure(EntityTypeBuilder<CustomerScore> builder)
    {
        // One row per customer: the customer's id is the key. It comes from the Customers module, never from a sequence.
        builder.HasKey(score => score.CustomerId);
        builder.Property(score => score.CustomerId).ValueGeneratedNever();
    }
}
