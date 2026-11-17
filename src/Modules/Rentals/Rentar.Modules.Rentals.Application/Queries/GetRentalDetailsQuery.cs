using System;
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
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// Rentals together with their car, customer, user and the customer's company; a cancelled rental, and one whose car,
/// customer or user is gone, is left out, and the company name is <c>null</c> for a customer with no company.
/// Throws <see cref="UnavailableException"/> during the 22:00 maintenance hour.
/// </summary>
public sealed record GetRentalDetailsQuery;

public sealed class GetRentalDetailsQueryHandler(
    IRentalDal rentalDal,
    ICustomersModuleApi customersModule,
    IIdentityModuleApi identityModule,
    IFleetModuleApi fleetModule,
    ICompaniesModuleApi companiesModule,
    TimeProvider timeProvider) : IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>>
{
    public async Task<List<RentalDetailResponse>> HandleAsync(GetRentalDetailsQuery query, CancellationToken cancellationToken = default)
    {
        if (timeProvider.GetLocalNow().Hour == 22)
        {
            throw new UnavailableException(Messages.MaintenanceTime);
        }

        var rentals = await rentalDal.GetAllAsync(r => r.Status != RentalStatus.Cancelled, cancellationToken);

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

        return details;
    }
}
