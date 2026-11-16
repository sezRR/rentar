using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>Answers the customer, or <c>null</c> when there is none with that id.</summary>
public sealed record GetCustomerByIdQuery(int Id);

public sealed class GetCustomerByIdQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomerByIdQuery, Customer>
{
    public Task<Customer> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        return customerDal.GetAsync(c => c.Id == query.Id, cancellationToken);
    }
}
