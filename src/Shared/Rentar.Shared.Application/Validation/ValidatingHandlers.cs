using FluentValidation;
using FluentValidation.Results;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Validation;

/// <summary>
/// Runs every validator of a use case and throws <see cref="ValidationException"/> with all their failures together.
/// A use case without a validator passes.
/// </summary>
internal static class UseCaseValidation
{
    public static async Task ValidateAsync<TUseCase>(
        IEnumerable<IValidator<TUseCase>> validators,
        TUseCase useCase,
        CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(useCase, cancellationToken);
            failures.AddRange(result.Errors);
        }

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
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await UseCaseValidation.ValidateAsync(validators, command, cancellationToken);
        await inner.HandleAsync(command, cancellationToken);
    }
}

/// <summary>Validates the command before the handler it wraps runs.</summary>
public sealed class ValidatingCommandHandler<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await UseCaseValidation.ValidateAsync(validators, command, cancellationToken);
        return await inner.HandleAsync(command, cancellationToken);
    }
}

/// <summary>Validates the query before the handler it wraps runs.</summary>
public sealed class ValidatingQueryHandler<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResult>
{
    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        await UseCaseValidation.ValidateAsync(validators, query, cancellationToken);
        return await inner.HandleAsync(query, cancellationToken);
    }
}
