using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Queries;

/// <summary>Every customer; throws <see cref="UnavailableException"/> during the 22:00 maintenance hour.</summary>
public sealed record GetCustomersQuery;

public sealed class GetCustomersQueryHandler(ICustomerDal customerDal, TimeProvider timeProvider) : IQueryHandler<GetCustomersQuery, List<Customer>>
{
    public async Task<List<Customer>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        if (timeProvider.GetLocalNow().Hour == 22)
        {
            throw new UnavailableException(Messages.MaintenanceTime);
        }

        return await customerDal.GetAllAsync(cancellationToken);
    }
}
