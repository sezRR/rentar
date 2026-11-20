using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>The customers that belong to the Identity user <see cref="UserId"/>, or one page of them.</summary>
public sealed record GetCustomersByUserQuery(int UserId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCustomersByUserQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomersByUserQuery, Page<Customer>>
{
    public Task<Page<Customer>> HandleAsync(GetCustomersByUserQuery query, CancellationToken cancellationToken = default)
    {
        return customerDal.GetPageAsync(c => c.UserId == query.UserId, query.Page, cancellationToken);
    }
}
