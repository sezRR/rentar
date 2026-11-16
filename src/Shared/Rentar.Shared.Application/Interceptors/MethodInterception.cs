using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Interceptors;

/// <summary>
/// An aspect made of hooks around the handler: before it starts, and once it has succeeded or failed.
/// </summary>
public abstract class MethodInterception : MethodInterceptionBaseAttribute
{
    protected virtual void OnBefore(IInvocation invocation) { }
    protected virtual void OnAfter(IInvocation invocation) { }
    protected virtual void OnException(IInvocation invocation, Exception e) { }
    protected virtual void OnSuccess(IInvocation invocation) { }

    protected override async Task<object> InterceptAsync(IInvocation invocation, Func<Task<object>> proceed)
    {
        OnBefore(invocation);

        object result;
        try
        {
            result = await proceed();
        }
        catch (Exception e)
        {
            OnException(invocation, e);
            throw;
        }

        OnSuccess(invocation);
        OnAfter(invocation);
        return result;
    }
}
