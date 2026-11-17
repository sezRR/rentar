using FluentValidation;
using Rentar.Modules.Fleet.Domain;

namespace Rentar.Modules.Fleet.Application.Brands;

/// <summary>
/// <see cref="Brand"/>'s invariants, checked up front by every use case that takes a brand, so the caller hears
/// about every field that breaks one at once. The brand's constructor enforces them again, whoever builds it.
/// </summary>
internal static class BrandRules
{
    public static IRuleBuilderOptions<T, string> ValidBrandName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MinimumLength(Brand.NameMinLength);
    }
}
