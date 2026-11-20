using Castle.DynamicProxy;
using Rentar.Shared.Application.Interceptors;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Aspects.Performance;

/// <summary>
/// Reports a call that completes but takes longer than <c>interval</c> seconds, measured until its task finishes.
/// </summary>
/// <remarks>
/// One aspect instance serves every call to its handler, concurrent ones included, so the start time is kept per
/// call rather than in a field.
/// </remarks>
public class PerformanceAspect(int interval) : MethodInterception
{
    private readonly int _interval = interval;

    protected override async Task<object> InterceptAsync(IInvocation invocation, Func<Task<object>> proceed)
    {
        var startedAt = Stopwatch.GetTimestamp();

        var result = await proceed();

        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        if (elapsed.TotalSeconds > _interval)
        {
            // The handler class, not the shared handler interface the proxy was called through.
            var handler = (invocation.TargetType ?? invocation.Method.DeclaringType).FullName;
            Debug.WriteLine($"Performance : {handler}.{invocation.Method.Name}-->{elapsed.TotalSeconds}");
        }

        return result;
    }
}
