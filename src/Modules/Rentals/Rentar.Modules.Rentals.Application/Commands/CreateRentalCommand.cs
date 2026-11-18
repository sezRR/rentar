using FluentValidation;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Fleet.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Contracts.Events;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// Books a rental and publishes <see cref="RentalCreatedEvent"/>, on which Payments takes its payment; answers the
/// stored rental, <see cref="RentalStatus.Pending"/> until the payment's outcome confirms or cancels it. Throws
/// <see cref="BusinessRuleException"/> when the Fleet module does not know the car or the Customers module does not know
/// the customer, and <see cref="ConflictException"/> when an active rental overlaps the car's or customer's period.
/// The dates are local times, so they carry no UTC offset; touching endpoints count as overlap.
/// </summary>
public sealed record CreateRentalCommand(int CarId, int CustomerId, DateTime RentDate, DateTime ReturnDate);

public sealed class CreateRentalCommandValidator : UseCaseValidator<CreateRentalCommand>
{
    public CreateRentalCommandValidator()
    {
        RuleFor(r => r.RentDate).NotEmpty();
        RuleFor(r => r.ReturnDate).ValidReturnDate(r => r.RentDate);
    }
}

public sealed class CreateRentalCommandHandler(
    IRentalDal rentalDal,
    IFleetModuleApi fleetModule,
    ICustomersModuleApi customersModule,
    IEventPublisher eventPublisher) : ICommandHandler<CreateRentalCommand, Rental>
{
    // Exclusion constraints arbitrate concurrent bookings; read committed lets a losing insert report that conflict
    // rather than aborting a serializable snapshot. The rental and its outbox event still commit together.
    [TransactionScopeAspect(IsolationLevel.ReadCommitted)]
    public async Task<Rental> HandleAsync(CreateRentalCommand command, CancellationToken cancellationToken = default)
    {
        // The car and the customer live in other modules, so no foreign key guards the references; those modules are
        // asked instead.
        var cars = await fleetModule.GetCarsAsync([command.CarId], cancellationToken);
        if (cars.Count == 0)
        {
            throw new BusinessRuleException(Messages.CarNotFound);
        }

        var customers = await customersModule.GetCustomersAsync([command.CustomerId], cancellationToken);
        if (customers.Count == 0)
        {
            throw new BusinessRuleException(Messages.CustomerNotFound);
        }

        if (await rentalDal.AnyAsync(
            r => r.CarId == command.CarId
                && r.Status != RentalStatus.Cancelled
                && r.ReturnDate >= command.RentDate
                && r.RentDate <= command.ReturnDate,
            cancellationToken))
        {
            throw new ConflictException(Messages.CarNotAvailable);
        }

        // Customers enforces one account per user, so reserving the customer also reserves that user's period.
        if (await rentalDal.AnyAsync(
            r => r.CustomerId == command.CustomerId
                && r.Status != RentalStatus.Cancelled
                && r.ReturnDate >= command.RentDate
                && r.RentDate <= command.ReturnDate,
            cancellationToken))
        {
            throw new ConflictException(Messages.CustomerNotAvailable);
        }

        var rental = new Rental(command.CarId, command.CustomerId, command.RentDate, command.ReturnDate);

        await rentalDal.AddAsync(rental, cancellationToken);

        await eventPublisher.PublishAsync(
            new RentalCreatedEvent(rental.Id, rental.CarId, rental.CustomerId, rental.RentDate, rental.ReturnDate),
            cancellationToken);

        return rental;
    }
}
