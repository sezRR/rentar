using JetBrains.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.UseCases;

/// <summary>
/// Carries out one use case that changes state, described by <typeparamref name="TCommand"/>.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Carries out one use case that changes state and hands back what the caller needs from it, e.g. the created record.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
