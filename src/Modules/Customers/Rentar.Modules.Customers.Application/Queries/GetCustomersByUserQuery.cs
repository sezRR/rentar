using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>The customers that belong to the Identity user <see cref="UserId"/>.</summary>
public sealed record GetCustomersByUserQuery(int UserId);

public sealed class GetCustomersByUserQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomersByUserQuery, List<Customer>>
{
    public Task<List<Customer>> HandleAsync(GetCustomersByUserQuery query, CancellationToken cancellationToken = default)
    {
        return customerDal.GetAllAsync(c => c.UserId == query.UserId, cancellationToken);
    }
}
