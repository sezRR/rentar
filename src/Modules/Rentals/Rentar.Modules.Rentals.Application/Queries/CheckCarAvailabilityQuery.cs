using FluentValidation;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// Whether the car is free for the whole period, i.e. no rental of it overlaps the period. A cancelled rental holds no car.
/// </summary>
public sealed record CheckCarAvailabilityQuery(int CarId, DateTime RentDate, DateTime ReturnDate);

public sealed class CheckCarAvailabilityQueryValidator : UseCaseValidator<CheckCarAvailabilityQuery>
{
    public CheckCarAvailabilityQueryValidator()
    {
        RuleFor(r => r.RentDate).NotEmpty();
        RuleFor(r => r.ReturnDate).ValidReturnDate(r => r.RentDate);
    }
}

public sealed class CheckCarAvailabilityQueryHandler(IRentalDal rentalDal) : IQueryHandler<CheckCarAvailabilityQuery, bool>
{
    public async Task<bool> HandleAsync(CheckCarAvailabilityQuery query, CancellationToken cancellationToken = default)
    {
        var overlaps = await rentalDal.AnyAsync(
            r => r.CarId == query.CarId
                && r.Status != RentalStatus.Cancelled
                && r.ReturnDate >= query.RentDate
                && r.RentDate <= query.ReturnDate,
            cancellationToken);

        return !overlaps;
    }
}
