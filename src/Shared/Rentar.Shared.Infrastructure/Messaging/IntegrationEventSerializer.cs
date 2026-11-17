using Rentar.Shared.Kernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Turns events into outbox content and back. Only the events the modules' Contracts declare are read back, so the
/// outbox never makes the application load a type named by stored data.
/// </summary>
public sealed class IntegrationEventSerializer
{
    private readonly Dictionary<string, Type> _eventTypes;

    public IntegrationEventSerializer(IEnumerable<IModuleMessageStore> stores)
    {
        _eventTypes = stores
            .SelectMany(store => store.ContractsAssembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IIntegrationEvent).IsAssignableFrom(type))
            .ToDictionary(type => type.FullName!, StringComparer.Ordinal);
    }

    public (string Type, string Content) Serialize(IIntegrationEvent integrationEvent)
    {
        var type = integrationEvent.GetType();
        if (!_eventTypes.ContainsKey(type.FullName!))
        {
            throw new InvalidOperationException($"{type.FullName} is not declared in the Contracts of a module with messaging.");
        }

        return (type.FullName!, JsonSerializer.Serialize(integrationEvent, type));
    }

    public IIntegrationEvent Deserialize(string type, string content)
    {
        if (!_eventTypes.TryGetValue(type, out var eventType))
        {
            throw new InvalidOperationException($"No module declares the event {type}.");
        }

        return (IIntegrationEvent)JsonSerializer.Deserialize(content, eventType)!;
    }
}
