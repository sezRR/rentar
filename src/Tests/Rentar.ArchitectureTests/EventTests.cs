using Autofac.Core;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Infrastructure.Modules;
using Rentar.Shared.Kernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// Modules react to each other through integration events: the publisher declares each event in its Contracts, every
/// subscriber adapts it to one of its own use cases, and events are published inside the publisher's transaction, so
/// an event is stored in the outbox exactly when the change it tells about commits.
/// </summary>
public class EventTests
{
    [Fact]
    public void EveryEvent_IsASealedRecord_NamedWithTheEventSuffix_InItsModulesContractsEventsNamespace()
    {
        var problems = new List<string>();

        foreach (var integrationEvent in Events())
        {
            var assembly = RentarAssembly.Parse(integrationEvent.Assembly.GetName().Name!);
            var expectedNamespace = $"Rentar.Modules.{assembly.Module}.Contracts.Events";

            if (assembly.Layer != "Contracts" || integrationEvent.Namespace != expectedNamespace)
            {
                problems.Add($"{integrationEvent.FullName} should be {expectedNamespace}.{integrationEvent.Name}");
            }

            if (!integrationEvent.IsSealed || integrationEvent.GetMethod("<Clone>$") is null)
            {
                problems.Add($"{integrationEvent.Name} should be a sealed record");
            }

            if (!integrationEvent.Name.EndsWith("Event", StringComparison.Ordinal))
            {
                problems.Add($"{integrationEvent.Name} should be named {integrationEvent.Name}Event");
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void EveryEvent_HasASubscriber()
    {
        var handled = UseCaseTests.ApplicationLayers()
            .SelectMany(EventHandlersIn)
            .SelectMany(HandledEventsOf)
            .ToHashSet();

        var unheard = Events()
            .Where(integrationEvent => !handled.Contains(integrationEvent))
            .Select(integrationEvent => $"{integrationEvent.Name} is published but no module reacts to it");

        Assert.Empty(unheard);
    }

    [Theory]
    [MemberData(nameof(UseCaseTests.ApplicationAssemblies), MemberType = typeof(UseCaseTests))]
    public void EveryEventHandler_IsSealed_AndHandlesOneEventNamedAfterIt(string assemblyName)
    {
        var problems = new List<string>();

        foreach (var handler in EventHandlersIn(RentarAssembly.Parse(assemblyName)))
        {
            var handled = HandledEventsOf(handler).ToList();
            if (handled.Count != 1)
            {
                problems.Add($"{handler.Name} handles {handled.Count} events instead of one");
                continue;
            }

            if (!handler.IsSealed)
            {
                problems.Add($"{handler.Name} is not sealed");
            }

            var expected = $"{assemblyName}.EventHandlers.{handled[0].Name}Handler";
            if (handler.FullName != expected)
            {
                problems.Add($"{handler.FullName} should be {expected}");
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void EveryEventHandler_IsRegisteredByItsModule()
    {
        var registry = UseCaseTests.ModuleContainer.Value.ComponentRegistry;

        var missing = UseCaseTests.ApplicationLayers()
            .SelectMany(EventHandlersIn)
            .SelectMany(handler => HandledEventsOf(handler).Select(handled => (Handler: handler, Event: handled)))
            .Where(pair => !registry
                .RegistrationsFor(new TypedService(typeof(IIntegrationEventHandler<>).MakeGenericType(pair.Event)))
                .Any(registration => registration.Activator.LimitType == pair.Handler))
            .Select(pair => pair.Handler.FullName);

        Assert.Empty(missing);
    }

    /// <summary>
    /// Outside a transaction the outbox write would not commit or roll back with the use case's own, so whatever
    /// publishes is a use case handler with <see cref="TransactionScopeAspect"/>. The publisher checks it at runtime too.
    /// </summary>
    [Theory]
    [MemberData(nameof(UseCaseTests.ApplicationAssemblies), MemberType = typeof(UseCaseTests))]
    public void EveryPublisher_IsAUseCaseHandler_ThatRunsInATransaction(string assemblyName)
    {
        var publishers = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.GetConstructors()
                .Any(constructor => constructor.GetParameters().Any(parameter => parameter.ParameterType == typeof(IEventPublisher))));

        var problems = publishers
            .Where(publisher => !UseCaseRegistrationExtensions.HandledInterfacesOf(publisher).Any()
                || publisher.GetMethod("HandleAsync")?.IsDefined(typeof(TransactionScopeAspect), inherit: true) != true)
            .Select(publisher => $"{publisher.Name} publishes events, so it should be a use case handler whose HandleAsync has [TransactionScopeAspect]");

        Assert.Empty(problems);
    }

    private static IEnumerable<Type> Events()
    {
        return RentarAssembly.All
            .Where(assembly => assembly.IsModule)
            .SelectMany(assembly => assembly.Load().GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IIntegrationEvent).IsAssignableFrom(type));
    }

    private static IEnumerable<Type> EventHandlersIn(RentarAssembly assembly)
    {
        return assembly.Load().GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && HandledEventsOf(type).Any());
    }

    /// <summary>The events a type handles, from the closed <see cref="IIntegrationEventHandler{TEvent}"/> it implements.</summary>
    private static IEnumerable<Type> HandledEventsOf(Type type)
    {
        return type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
            .Select(i => i.GetGenericArguments()[0]);
    }
}
