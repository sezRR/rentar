using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Shared.Application.Caching;
using Rentar.Shared.Application.Interceptors;
using Rentar.Shared.Application.IoC;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Shared.Application.Aspects.Caching;

public class CacheAspect(int duration = 60) : MethodInterception
{
    private readonly int _duration = duration;
    private readonly ICacheManager _cacheManager = ServiceTool.ServiceProvider.GetRequiredService<ICacheManager>();

    protected override async Task<object> InterceptAsync(IInvocation invocation, Func<Task<object>> proceed)
    {
        var key = KeyOf(invocation);
        if (_cacheManager.IsAdd(key))
        {
            return _cacheManager.Get(key);
        }

        // The handler's result is cached once it has finished, never the task that is still producing it.
        var result = await proceed();
        _cacheManager.Add(key, result, _duration);
        return result;
    }

    /// <summary>
    /// Keyed by the handler class (e.g. "...Cars.Queries.GetCarsByBrandQueryHandler.HandleAsync") rather than the shared
    /// handler interface, so a CacheRemoveAspect pattern can name the use cases it invalidates. The cancellation token
    /// says nothing about what was asked, so it is not part of the key.
    /// </summary>
    private static string KeyOf(IInvocation invocation)
    {
        var methodName = $"{(invocation.TargetType ?? invocation.Method.ReflectedType).FullName}.{invocation.Method.Name}";
        var arguments = invocation.Arguments
            .Where(argument => argument is not CancellationToken)
            .Select(argument => argument?.ToString() ?? "<Null>");

        return $"{methodName}({string.Join(",", arguments)})";
    }
}
