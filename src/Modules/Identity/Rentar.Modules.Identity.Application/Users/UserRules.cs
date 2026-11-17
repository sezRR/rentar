using FluentValidation;

namespace Rentar.Modules.Identity.Application.Users;

/// <summary>
/// What a valid user looks like, shared by every use case that takes one, so the caller hears about every field that
/// breaks it at once. <see cref="Domain.User"/>'s constructor enforces the name and email rules again, whoever builds
/// one; the password never reaches the domain, only its hash.
/// </summary>
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
