using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>Every customer, or one page of them.</summary>
public sealed record GetCustomersQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetCustomersQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomersQuery, Page<Customer>>
{
    public Task<Page<Customer>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        return customerDal.GetPageAsync(query.Page, cancellationToken);
    }
}
