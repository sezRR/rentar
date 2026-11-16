using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// The rentals booked by the customers of the Companies company <see cref="CompanyId"/>, for its employee
/// <see cref="ActorUserId"/> holding <see cref="CompanyPermissions.RentalsRead"/>. Throws
/// <see cref="NotFoundException"/> when the actor does not work there and <see cref="ForbiddenException"/> when their
/// roles lack the permission.
/// </summary>
public sealed record GetCompanyRentalsQuery(int ActorUserId, int CompanyId);

public sealed class GetCompanyRentalsQueryHandler(
    IRentalDal rentalDal,
    ICompaniesModuleApi companiesModule,
    ICustomersModuleApi customersModule) : IQueryHandler<GetCompanyRentalsQuery, List<Rental>>
{
    public async Task<List<Rental>> HandleAsync(GetCompanyRentalsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await companiesModule.GetEmployeeAsync(query.CompanyId, query.ActorUserId, cancellationToken)
            ?? throw new NotFoundException(Messages.CompanyNotFound(query.CompanyId));

        if (!actor.Permissions.Contains(CompanyPermissions.RentalsRead))
        {
            throw new ForbiddenException(Messages.PermissionMissing(CompanyPermissions.RentalsRead));
        }

        // A rental only holds the id of its customer; which customers rent for the company is the Customers module's to say.
        var customerIds = (await customersModule.GetCompanyCustomersAsync(query.CompanyId, cancellationToken))
            .Select(c => c.CustomerId)
            .ToArray();

        if (customerIds.Length == 0)
        {
            return [];
        }

        var rentals = await rentalDal.GetAllAsync(r => customerIds.Contains(r.CustomerId), cancellationToken);
        return rentals.OrderBy(r => r.Id).ToList();
    }
}
