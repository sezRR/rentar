using FluentValidation;

namespace Rentar.Modules.Identity.Application.Users;

/// <summary>What a valid user looks like, shared by every use case that takes one.</summary>
internal static class UserRules
{
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty();
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().EmailAddress();
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty();
    }
}
