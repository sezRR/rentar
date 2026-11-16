using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentar.Modules.Customers.Domain;

namespace Rentar.Modules.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // The user lives in the Identity schema, so there is no foreign key to index it; customers are listed by user.
        builder.HasIndex(customer => customer.UserId);

        // Same for the company in the Companies schema; a company's rentals are found through its customers.
        builder.HasIndex(customer => customer.CompanyId);
    }
}
