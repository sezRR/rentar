using FluentValidation;

namespace Rentar.Modules.Fleet.Application.Brands;

/// <summary>What a valid brand looks like, shared by every use case that takes one.</summary>
internal static class BrandRules
{
    public static IRuleBuilderOptions<T, string> ValidBrandName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MinimumLength(2);
    }
}
