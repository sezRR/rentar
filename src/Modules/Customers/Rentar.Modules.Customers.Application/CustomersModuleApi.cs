using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Customers.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application;

public class CustomersModuleApi(ICustomerDal customerDal) : ICustomersModuleApi
{
    public async Task<IReadOnlyList<CustomerSummary>> GetCustomersAsync(
        IReadOnlyCollection<int> customerIds,
        CancellationToken cancellationToken = default)
    {
        if (customerIds.Count == 0)
        {
            return [];
        }

        var ids = customerIds.ToArray();
        var customers = await customerDal.GetAllAsync(c => ids.Contains(c.Id), cancellationToken);
        return customers.ConvertAll(ToSummary);
    }

    public async Task<IReadOnlyList<CustomerSummary>> GetCompanyCustomersAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var customers = await customerDal.GetAllAsync(c => c.CompanyId == companyId, cancellationToken);
        return customers.ConvertAll(ToSummary);
    }

    private static CustomerSummary ToSummary(Customer customer)
    {
        return new CustomerSummary(customer.Id, customer.UserId, customer.CompanyName, customer.CompanyId);
    }
}
