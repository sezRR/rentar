using Microsoft.Extensions.DependencyInjection;
using Rentar.Shared.Application.Caching;
using Rentar.Shared.Application.Security;
using Rentar.Shared.Infrastructure.Caching;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Security;
using System;

namespace Rentar.Shared.Infrastructure;

public static class SharedInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Services the shared aspects resolve, the clock modules read the time from, and the messaging modules publish
    /// and receive integration events through.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheManager, MemoryCacheManager>();
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton(TimeProvider.System);
        services.AddMessaging();

        return services;
    }
}
