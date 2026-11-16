using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Contracts;
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
        return customers.ConvertAll(c => new CustomerSummary(c.Id, c.UserId, c.CompanyName));
    }
}
