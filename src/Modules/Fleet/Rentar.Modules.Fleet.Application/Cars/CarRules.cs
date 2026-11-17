using FluentValidation;
using Rentar.Modules.Fleet.Domain;

namespace Rentar.Modules.Fleet.Application.Cars;

/// <summary>
/// <see cref="Car"/>'s invariants, checked up front by every use case that takes a car, so the caller hears about
/// every field that breaks one at once. The car's constructor enforces them again, whoever builds it.
/// </summary>
internal static class CarRules
{
    public static IRuleBuilderOptions<T, string> ValidDescription<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MinimumLength(Car.DescriptionMinLength);
    }

    public static IRuleBuilderOptions<T, int> ValidModelYear<T>(this IRuleBuilder<T, int> rule)
    {
        return rule.GreaterThan(0);
    }

    public static IRuleBuilderOptions<T, decimal> ValidDailyPrice<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule.GreaterThan(0);
    }
}
