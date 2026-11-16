using FluentValidation;
using Rentar.Modules.Customers.Contracts;
using Rentar.Modules.Fleet.Contracts;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Queries;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// Stores a new rental and answers it with its id set; throws <see cref="BusinessRuleException"/> when the Fleet module
/// does not know the car or the Customers module does not know the customer. It does not check that the car is free;
/// ask <see cref="CheckCarAvailabilityQuery"/> first. The dates are local times, so they carry no UTC offset.
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

public sealed class CreateRentalCommandHandler(IRentalDal rentalDal, IFleetModuleApi fleetModule, ICustomersModuleApi customersModule)
    : ICommandHandler<CreateRentalCommand, Rental>
{
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

        var rental = new Rental
        {
            CarId = command.CarId,
            CustomerId = command.CustomerId,
            RentDate = command.RentDate,
            ReturnDate = command.ReturnDate
        };

        await rentalDal.AddAsync(rental, cancellationToken);
        return rental;
    }
}
