using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// Cancels every rental of the Identity user <see cref="UserId"/>'s customer account that has not started yet. Rentals
/// that started, i.e. whose rent date has passed, stay as they are: the car is on the road already.
/// </summary>
public sealed record CancelUpcomingRentalsOfUserCommand(int UserId);

public sealed class CancelUpcomingRentalsOfUserCommandHandler(
    IRentalDal rentalDal,
    ICustomersModuleApi customersModule,
    TimeProvider timeProvider) : ICommandHandler<CancelUpcomingRentalsOfUserCommand>
{
    public async Task HandleAsync(CancelUpcomingRentalsOfUserCommand command, CancellationToken cancellationToken = default)
    {
        // A rental knows its customer only; which customer is the user's is the Customers module's to say.
        var customer = await customersModule.GetUserCustomerAsync(command.UserId, cancellationToken);
        if (customer is null)
        {
            return;
        }

        // Rental dates are local wall-clock times, so they are compared with the local time.
        var now = timeProvider.GetLocalNow().DateTime;
        var upcoming = await rentalDal.GetAllAsync(
            r => r.CustomerId == customer.CustomerId && r.RentDate > now && r.Status != RentalStatus.Cancelled,
            cancellationToken);

        foreach (var rental in upcoming)
        {
            await rentalDal.UpdateAsync(rental.Cancelled(), cancellationToken);
        }
    }
}
