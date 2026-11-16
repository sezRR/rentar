using FluentValidation;
using System;

namespace Rentar.Modules.Rentals.Application;

/// <summary>What a valid rental period looks like, shared by every use case that takes one.</summary>
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
