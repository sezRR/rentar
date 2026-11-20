using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Shared.Application.Events;
using System.Reflection;

namespace Rentar.Shared.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// The outbox publisher use cases publish through, the in-memory event bus, and the background delivery. Each module
    /// adds its own outbox and inbox with <see cref="AddModuleMessaging{TContext}"/>.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddOptions<OutboxOptions>().BindConfiguration(OutboxOptions.SectionName);

        services.AddSingleton<IntegrationEventSerializer>();
        services.AddSingleton<OutboxSignal>();
        services.AddSingleton<IEventPublisher, OutboxEventPublisher>();
        services.AddSingleton<IntegrationEventDispatcher>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    /// <summary>
    /// Gives the module an outbox and an inbox in its context's schema; the context maps them with
    /// <see cref="MessagingModelBuilderExtensions.MapMessaging"/>. The events declared in
    /// <paramref name="contractsAssembly"/> go to this outbox, and the event handlers in
    /// <paramref name="applicationAssembly"/> record what they handled in this inbox.
    /// </summary>
    public static IServiceCollection AddModuleMessaging<TContext>(
        this IServiceCollection services,
        Assembly contractsAssembly,
        Assembly applicationAssembly) where TContext : DbContext
    {
        services.AddSingleton<IModuleMessageStore>(serviceProvider => new ModuleMessageStore<TContext>(
            serviceProvider.GetRequiredService<IDbContextFactory<TContext>>(),
            contractsAssembly,
            applicationAssembly));

        return services;
    }
}
