using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Fleet.Contracts;
using Rentar.Modules.Identity.Contracts;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// The car, the customer's user and company, and the car's daily price as the other modules know them now, for a rental
/// to keep as it is booked. Throws <see cref="BusinessRuleException"/> when the Fleet module does not know the car, or
/// the Customers module the customer or the Identity module the customer's user.
/// </summary>
public sealed record GetRentalSnapshotQuery(int CarId, int CustomerId);

public sealed class GetRentalSnapshotQueryHandler(
    IFleetModuleApi fleetModule,
    ICustomersModuleApi customersModule,
    IIdentityModuleApi identityModule,
    ICompaniesModuleApi companiesModule) : IQueryHandler<GetRentalSnapshotQuery, RentalSnapshotResponse>
{
    public async Task<RentalSnapshotResponse> HandleAsync(GetRentalSnapshotQuery query, CancellationToken cancellationToken = default)
    {
        // The car and the customer live in other modules, so no foreign key guards the references; those modules are
        // asked instead.
        var cars = await fleetModule.GetCarsAsync([query.CarId], cancellationToken);
        if (cars.Count == 0)
        {
            throw new BusinessRuleException(Messages.CarNotFound);
        }

        var customers = await customersModule.GetCustomersAsync([query.CustomerId], cancellationToken);
        if (customers.Count == 0)
        {
            throw new BusinessRuleException(Messages.CustomerNotFound);
        }

        var customer = customers[0];
        var users = await identityModule.GetUsersAsync([customer.UserId], cancellationToken);
        if (users.Count == 0)
        {
            throw new BusinessRuleException(Messages.CustomerNotFound);
        }

        // Like a rental's details always did, a company the Companies module no longer knows leaves the name empty.
        string companyName = null;
        if (customer.CompanyId is { } companyId)
        {
            var companies = await companiesModule.GetCompaniesAsync([companyId], cancellationToken);
            companyName = companies.Count > 0 ? companies[0].Name : null;
        }

        var car = cars[0];
        var user = users[0];
        return new RentalSnapshotResponse(
            new RentalCarSnapshot(car.BrandId, car.BrandName, car.ColorId, car.ColorName, car.ModelYear, car.Description),
            new RentalCustomerSnapshot(user.FirstName, user.LastName, customer.CompanyId, companyName),
            car.DailyPrice);
    }
}
