using FluentValidation;

namespace Rentar.Modules.Fleet.Application.Cars;

/// <summary>What a valid car looks like, shared by every use case that takes one.</summary>
internal static class CarRules
{
    public static IRuleBuilderOptions<T, string> ValidDescription<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MinimumLength(2);
    }

    public static IRuleBuilderOptions<T, int> ValidModelYear<T>(this IRuleBuilder<T, int> rule)
    {
        return rule.NotEmpty();
    }

    public static IRuleBuilderOptions<T, decimal> ValidDailyPrice<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule.GreaterThan(0);
    }
}
