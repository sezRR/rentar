using FluentValidation;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Application.Queries;
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
/// <see cref="BusinessRuleException"/> when <see cref="GetRentalSnapshotQuery"/> does, as another module does not know
/// the car, the customer or its user, and <see cref="ConflictException"/> when an active rental overlaps the car's or
/// customer's period. The dates are local times, so they carry no UTC offset; touching endpoints count as overlap.
/// The rental keeps that snapshot of the car, the customer and the daily price, so later changes in those modules leave
/// it as it was booked.
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
    IQueryHandler<GetRentalSnapshotQuery, RentalSnapshotResponse> getSnapshot,
    IEventPublisher eventPublisher) : ICommandHandler<CreateRentalCommand, Rental>
{
    // Exclusion constraints arbitrate concurrent bookings; read committed lets a losing insert report that conflict
    // rather than aborting a serializable snapshot. The rental and its outbox event still commit together.
    [TransactionScopeAspect(IsolationLevel.ReadCommitted)]
    public async Task<Rental> HandleAsync(CreateRentalCommand command, CancellationToken cancellationToken = default)
    {
        var snapshot = await getSnapshot.HandleAsync(new GetRentalSnapshotQuery(command.CarId, command.CustomerId), cancellationToken);

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

        var rental = new Rental(
            command.CarId,
            command.CustomerId,
            command.RentDate,
            command.ReturnDate,
            snapshot.Car,
            snapshot.Customer,
            snapshot.DailyPrice);

        await rentalDal.AddAsync(rental, cancellationToken);

        await eventPublisher.PublishAsync(
            new RentalCreatedEvent(rental.Id, rental.CarId, rental.CustomerId, rental.RentDate, rental.ReturnDate),
            cancellationToken);

        return rental;
    }
}
