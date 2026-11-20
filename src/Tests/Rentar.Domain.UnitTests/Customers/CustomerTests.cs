using Rentar.Modules.Customers.Domain;
using Xunit;

namespace Rentar.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Theory]
    [InlineData(5)]
    [InlineData(null)]
    public void Constructor_KeepsWhatItWasGiven(int? companyId)
    {
        var customer = new Customer(userId: 9, companyId);

        Assert.Equal(9, customer.UserId);
        Assert.Equal(companyId, customer.CompanyId);
    }

    [Fact]
    public void WithoutCompany_IsTheSameCustomer_WithNoCompany()
    {
        var customer = new Customer(userId: 9, companyId: 5);

        var cleared = customer.WithoutCompany();

        Assert.Equal(customer.Id, cleared.Id);
        Assert.Equal(9, cleared.UserId);
        Assert.Null(cleared.CompanyId);
        Assert.Equal(5, customer.CompanyId);
    }
}
