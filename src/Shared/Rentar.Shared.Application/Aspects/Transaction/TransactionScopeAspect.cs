using Castle.DynamicProxy;
using Rentar.Shared.Application.Interceptors;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Rentar.Shared.Application.Aspects.Transaction;

/// <summary>
/// Runs the handler in an ambient transaction that commits only when the handler's task succeeds.
/// </summary>
public class TransactionScopeAspect : MethodInterception
{
    protected override async Task<object> InterceptAsync(IInvocation invocation, Func<Task<object>> proceed)
    {
        // Async flow carries the ambient transaction across the handler's awaits, which a default scope would lose.
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var result = await proceed();
        transactionScope.Complete();
        return result;
    }
}
