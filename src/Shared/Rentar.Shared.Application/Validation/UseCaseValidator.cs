using FluentValidation;
using JetBrains.Annotations;

namespace Rentar.Shared.Application.Validation;

/// <summary>
/// Checks the input of one use case, <typeparamref name="TUseCase"/>, before its handler runs: required values,
/// lengths, formats and how fields relate to each other. Nothing that needs stored data; the handler checks that.
/// </summary>
/// <remarks>
/// <c>RegisterUseCases</c> finds every validator and the handler decorators in <c>ValidatingHandlers</c> run it, so a
/// handler never opts in. Each property stops at its first failing rule, so a missing value is reported once, not
/// also as too short.
/// </remarks>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class UseCaseValidator<TUseCase> : AbstractValidator<TUseCase>
{
    protected UseCaseValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}
