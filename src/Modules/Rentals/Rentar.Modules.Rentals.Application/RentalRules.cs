using FluentValidation;
using System;

namespace Rentar.Modules.Rentals.Application;

/// <summary>
/// What a valid rental period looks like, shared by every use case that takes one, so the caller hears about every
/// field that breaks it at once. A <see cref="Domain.Rental"/>'s constructor enforces it again, whoever builds one.
/// </summary>
internal static class RentalRules
{
    /// <summary>A period that ends after it starts; <paramref name="rentDate"/> picks the start from the same use case.</summary>
    public static IRuleBuilderOptions<T, DateTime> ValidReturnDate<T>(
        this IRuleBuilder<T, DateTime> rule,
        Func<T, DateTime> rentDate)
    {
        return rule.NotEmpty()
            .Must((useCase, returnDate) => returnDate > rentDate(useCase))
            .WithMessage("'{PropertyName}' must be after the rent date.");
    }
}
