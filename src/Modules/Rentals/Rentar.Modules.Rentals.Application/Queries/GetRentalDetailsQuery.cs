using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Fleet.Contracts;
using Rentar.Modules.Identity.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// Rentals together with their car, customer, user and the customer's company; a cancelled rental, and one whose car,
/// customer or user is gone, is left out, and the company name is <c>null</c> for a customer with no company. Asked for
/// one page, it pages what is left: which rentals are left out is only known once the other modules have answered, so
/// every rental after the cursor is read.
/// </summary>
public sealed record GetRentalDetailsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetRentalDetailsQueryHandler(
    IRentalDal rentalDal,
    ICustomersModuleApi customersModule,
    IIdentityModuleApi identityModule,
    IFleetModuleApi fleetModule,
    ICompaniesModuleApi companiesModule) : IQueryHandler<GetRentalDetailsQuery, Page<RentalDetailResponse>>
{
    public async Task<Page<RentalDetailResponse>> HandleAsync(GetRentalDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var after = query.Page?.After;
        var rentals = await rentalDal.GetAllAsync(
            r => r.Status != RentalStatus.Cancelled && (after == null || r.Id > after),
            cancellationToken);

        // A rental only holds the ids of other modules' records, so their details come through those modules' public APIs.
        var customers = (await customersModule.GetCustomersAsync(rentals.Select(r => r.CustomerId).Distinct().ToList(), cancellationToken))
            .ToDictionary(c => c.CustomerId);
        var users = (await identityModule.GetUsersAsync(customers.Values.Select(c => c.UserId).Distinct().ToList(), cancellationToken))
            .ToDictionary(u => u.UserId);
        var companyIds = customers.Values.Select(c => c.CompanyId).OfType<int>().Distinct().ToList();
        var companies = (await companiesModule.GetCompaniesAsync(companyIds, cancellationToken))
            .ToDictionary(c => c.CompanyId);
        var cars = (await fleetModule.GetCarsAsync(rentals.Select(r => r.CarId).Distinct().ToList(), cancellationToken))
            .ToDictionary(c => c.CarId);

        var details = new List<RentalDetailResponse>();
        foreach (var rental in rentals)
        {
            // A rental is listed only while its customer, the customer's user and the car all still exist.
            if (!customers.TryGetValue(rental.CustomerId, out var customer)
                || !users.TryGetValue(customer.UserId, out var user)
                || !cars.TryGetValue(rental.CarId, out var car))
            {
                continue;
            }

            details.Add(new RentalDetailResponse
            {
                RentalId = rental.Id,
                CarId = rental.CarId,
                RentDate = rental.RentDate,
                ReturnDate = rental.ReturnDate,
                BrandName = car.BrandName,
                ColorName = car.ColorName,
                Description = car.Description,
                DailyPrice = car.DailyPrice,
                ModelYear = car.ModelYear,
                CompanyName = customer.CompanyId is { } companyId ? companies.GetValueOrDefault(companyId)?.Name : null,
                UserName = user.FirstName
            });
        }

        return Page.Of(details, detail => detail.RentalId, query.Page);
    }
}
