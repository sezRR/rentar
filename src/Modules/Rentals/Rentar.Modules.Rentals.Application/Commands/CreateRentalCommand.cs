using FluentValidation;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// Stores a new rental and answers it with its id set. It does not check that the car is free; ask
/// <see cref="CheckCarAvailabilityQuery"/> first. The dates are local times, so they carry no UTC offset.
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

public sealed class CreateRentalCommandHandler(IRentalDal rentalDal) : ICommandHandler<CreateRentalCommand, Rental>
{
    public Rental Handle(CreateRentalCommand command)
    {
        var rental = new Rental
        {
            CarId = command.CarId,
            CustomerId = command.CustomerId,
            RentDate = command.RentDate,
            ReturnDate = command.ReturnDate
        };

        rentalDal.Add(rental);
        return rental;
    }
}
