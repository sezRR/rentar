using FluentValidation;

namespace Rentar.Modules.Companies.Application.Companies;

/// <summary>
/// What a valid company looks like, shared by every use case that takes one, so the caller hears about every field
/// that breaks it at once. <see cref="Domain.Company"/> enforces it again, however it is built or renamed.
/// </summary>
internal static class CompanyRules
{
    public static IRuleBuilderOptions<T, string> ValidCompanyName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty();
    }
}
