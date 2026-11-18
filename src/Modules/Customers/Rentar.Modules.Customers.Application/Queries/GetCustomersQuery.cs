using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>Every customer.</summary>
public sealed record GetCustomersQuery;

public sealed class GetCustomersQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomersQuery, List<Customer>>
{
    public async Task<List<Customer>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        return await customerDal.GetAllAsync(cancellationToken);
    }
}
