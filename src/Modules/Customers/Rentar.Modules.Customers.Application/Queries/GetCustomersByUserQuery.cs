using System.Collections.Generic;
using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>The customers that belong to the Identity user <see cref="UserId"/>.</summary>
public sealed record GetCustomersByUserQuery(int UserId);

public sealed class GetCustomersByUserQueryHandler(ICustomerDal customerDal) : IQueryHandler<GetCustomersByUserQuery, List<Customer>>
{
    public List<Customer> Handle(GetCustomersByUserQuery query)
    {
        return customerDal.GetAll(c => c.UserId == query.UserId);
    }
}
