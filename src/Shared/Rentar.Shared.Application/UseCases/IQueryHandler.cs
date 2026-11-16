using JetBrains.Annotations;

namespace Rentar.Shared.Application.UseCases;

/// <summary>
/// Answers one use case that only reads, described by <typeparamref name="TQuery"/>. It changes nothing.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
