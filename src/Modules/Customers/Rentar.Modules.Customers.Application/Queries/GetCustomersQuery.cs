using System;
using System.Collections.Generic;
using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>Every customer; throws <see cref="UnavailableException"/> during the 22:00 maintenance hour.</summary>
public sealed record GetCustomersQuery;

public sealed class GetCustomersQueryHandler(ICustomerDal customerDal, TimeProvider timeProvider) : IQueryHandler<GetCustomersQuery, List<Customer>>
{
    public List<Customer> Handle(GetCustomersQuery query)
    {
        if (timeProvider.GetLocalNow().Hour == 22)
        {
            throw new UnavailableException(Messages.MaintenanceTime);
        }

        return customerDal.GetAll();
    }
}
