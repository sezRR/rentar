using System;
using System.Collections.Generic;
using System.Linq;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Fleet.Contracts;
using Rentar.Modules.Identity.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// Rentals together with their car, customer and user; a rental whose car, customer or user is gone is left out.
/// Throws <see cref="UnavailableException"/> during the 22:00 maintenance hour.
/// </summary>
public sealed record GetRentalDetailsQuery;

public sealed class GetRentalDetailsQueryHandler(
    IRentalDal rentalDal,
    ICustomersModuleApi customersModule,
    IIdentityModuleApi identityModule,
    IFleetModuleApi fleetModule,
    TimeProvider timeProvider) : IQueryHandler<GetRentalDetailsQuery, List<RentalDetailResponse>>
{
    public List<RentalDetailResponse> Handle(GetRentalDetailsQuery query)
    {
        if (timeProvider.GetLocalNow().Hour == 22)
        {
            throw new UnavailableException(Messages.MaintenanceTime);
        }

        var rentals = rentalDal.GetAll();

        // A rental only holds the ids of other modules' records, so their details come through those modules' public APIs.
        var customers = customersModule.GetCustomers(rentals.Select(r => r.CustomerId).Distinct().ToList())
            .ToDictionary(c => c.CustomerId);
        var users = identityModule.GetUsers(customers.Values.Select(c => c.UserId).Distinct().ToList())
            .ToDictionary(u => u.UserId);
        var cars = fleetModule.GetCars(rentals.Select(r => r.CarId).Distinct().ToList())
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
                CompanyName = customer.CompanyName,
                UserName = user.FirstName
            });
        }

        return details;
    }
}
