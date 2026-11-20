using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Interceptors;

/// <summary>
/// An aspect: an attribute on a use case handler, or on its <c>HandleAsync</c>, that runs around every call to it.
/// </summary>
/// <remarks>
/// Handlers are asynchronous, so an aspect runs around the task a handler returns, not just the call that starts it:
/// <see cref="InterceptAsync"/> awaits the handler and sees its result or its exception.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class MethodInterceptionBaseAttribute : Attribute, IInterceptor
{
    public int Priority { get; set; }

    /// <summary>
    /// Runs the aspect around one call. <paramref name="proceed"/> runs the rest of the chain, the handler last, and
    /// answers the handler's result, or <c>null</c> for a handler that answers nothing.
    /// </summary>
    protected abstract Task<object> InterceptAsync(IInvocation invocation, Func<Task<object>> proceed);

    void IInterceptor.Intercept(IInvocation invocation)
    {
        AsyncInvocation.Intercept(invocation, InterceptAsync);
    }
}
