using FluentValidation;

namespace Rentar.Modules.Companies.Application.Companies;

/// <summary>What a valid company looks like, shared by every use case that takes one.</summary>
internal static class CompanyRules
{
    public static IRuleBuilderOptions<T, string> ValidCompanyName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty();
    }
}
