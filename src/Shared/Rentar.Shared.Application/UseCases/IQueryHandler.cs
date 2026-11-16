using JetBrains.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.UseCases;

/// <summary>
/// Answers one use case that only reads, described by <typeparamref name="TQuery"/>. It changes nothing.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
