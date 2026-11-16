using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Interceptors;

/// <summary>
/// Lets an aspect await the handler it wraps. Castle calls an interceptor synchronously, and the handler's task is
/// usually still running when the call returns, so the interceptor hands the caller a task of its own instead: one
/// that awaits the handler's task and finishes with the same result.
/// </summary>
internal static class AsyncInvocation
{
    private static readonly MethodInfo CastMethod = typeof(AsyncInvocation).GetMethod(nameof(CastAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly ConcurrentDictionary<Type, Func<Task<object>, Task>> Casts = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo> ResultProperties = new();

    public static void Intercept(IInvocation invocation, Func<IInvocation, Func<Task<object>>, Task<object>> intercept)
    {
        var resultType = ResultTypeOf(invocation.Method);

        // Proceed may only be called while Intercept runs; the captured form can also be called after an await.
        var proceedInfo = invocation.CaptureProceedInfo();

        async Task<object> Proceed()
        {
            proceedInfo.Invoke();

            // Read at once: the next interceptor or the handler has just set it, and this interceptor replaces it below.
            var task = (Task)invocation.ReturnValue;
            await task;

            return resultType is null ? null : ResultProperties.GetOrAdd(resultType, ResultPropertyOf).GetValue(task);
        }

        var result = intercept(invocation, Proceed);

        invocation.ReturnValue = resultType is null ? result : Casts.GetOrAdd(resultType, CastFor)(result);
    }

    /// <summary>
    /// <c>T</c> for a method returning <c>Task&lt;T&gt;</c>, <c>null</c> for one returning <see cref="Task"/>.
    /// </summary>
    private static Type ResultTypeOf(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(Task))
        {
            return null;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        throw new InvalidOperationException(
            $"{method.DeclaringType?.FullName}.{method.Name} returns {returnType.Name}; aspects only run around methods that return a Task.");
    }

    private static PropertyInfo ResultPropertyOf(Type resultType)
    {
        return typeof(Task<>).MakeGenericType(resultType).GetProperty(nameof(Task<object>.Result))!;
    }

    private static Func<Task<object>, Task> CastFor(Type resultType)
    {
        return CastMethod.MakeGenericMethod(resultType).CreateDelegate<Func<Task<object>, Task>>();
    }

    private static async Task<T> CastAsync<T>(Task<object> task)
    {
        return (T)await task;
    }
}
