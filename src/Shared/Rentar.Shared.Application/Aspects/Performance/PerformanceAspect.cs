using Castle.DynamicProxy;
using Rentar.Shared.Application.Interceptors;
using System.Diagnostics;

namespace Rentar.Shared.Application.Aspects.Performance;

/// <summary>
/// Reports a call that completes but takes longer than <c>interval</c> seconds.
/// </summary>
/// <remarks>
/// One aspect instance serves every call to its handler, concurrent ones included, so the start time is kept per
/// call rather than in a field.
/// </remarks>
public class PerformanceAspect(int interval) : MethodInterception
{
    private readonly int _interval = interval;

    public override void Intercept(IInvocation invocation)
    {
        var startedAt = Stopwatch.GetTimestamp();

        invocation.Proceed();

        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        if (elapsed.TotalSeconds > _interval)
        {
            // The handler class, not the shared handler interface the proxy was called through.
            var handler = (invocation.TargetType ?? invocation.Method.DeclaringType).FullName;
            Debug.WriteLine($"Performance : {handler}.{invocation.Method.Name}-->{elapsed.TotalSeconds}");
        }
    }
}
