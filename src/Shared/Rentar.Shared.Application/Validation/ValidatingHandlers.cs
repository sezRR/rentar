using FluentValidation;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Linq;

namespace Rentar.Shared.Application.Validation;

/// <summary>
/// Runs every validator of a use case and throws <see cref="ValidationException"/> with all their failures together.
/// A use case without a validator passes.
/// </summary>
internal static class UseCaseValidation
{
    public static void Validate<TUseCase>(IEnumerable<IValidator<TUseCase>> validators, TUseCase useCase)
    {
        var failures = validators
            .SelectMany(validator => validator.Validate(useCase).Errors)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}

/// <summary>Validates the command before the handler it wraps runs.</summary>
public sealed class ValidatingCommandHandler<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
{
    public void Handle(TCommand command)
    {
        UseCaseValidation.Validate(validators, command);
        inner.Handle(command);
    }
}

/// <summary>Validates the command before the handler it wraps runs.</summary>
public sealed class ValidatingCommandHandler<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResult>
{
    public TResult Handle(TCommand command)
    {
        UseCaseValidation.Validate(validators, command);
        return inner.Handle(command);
    }
}

/// <summary>Validates the query before the handler it wraps runs.</summary>
public sealed class ValidatingQueryHandler<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResult>
{
    public TResult Handle(TQuery query)
    {
        UseCaseValidation.Validate(validators, query);
        return inner.Handle(query);
    }
}
