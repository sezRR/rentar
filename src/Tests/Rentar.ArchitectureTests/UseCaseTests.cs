using Autofac;
using FluentValidation;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Infrastructure.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// Application logic lives in use cases: a record describing the use case and one sealed handler for it, next to
/// each other. No service class is left to gather unrelated operations.
/// </summary>
public class UseCaseTests
{
    private static readonly Lazy<IContainer> ModuleContainer = new(BuildModuleContainer);

    public static TheoryData<string> ApplicationAssemblies()
    {
        return new TheoryData<string>(ApplicationLayers().Select(assembly => assembly.Name));
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblies))]
    public void ApplicationLayer_HasNoManagersOrServices(string assemblyName)
    {
        var leftovers = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.Name.EndsWith("Manager", StringComparison.Ordinal)
                || (type.IsInterface && type.Name.StartsWith('I') && type.Name.EndsWith("Service", StringComparison.Ordinal)))
            .Select(type => type.FullName);

        Assert.Empty(leftovers);
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblies))]
    public void EveryHandler_IsSealed_AndHandlesOneRecordNamedAfterIt(string assemblyName)
    {
        var problems = new List<string>();

        foreach (var handler in HandlersIn(RentarAssembly.Parse(assemblyName)))
        {
            var handled = UseCaseRegistrationExtensions.HandledInterfacesOf(handler).ToList();
            if (handled.Count != 1)
            {
                problems.Add($"{handler.Name} handles {handled.Count} use cases instead of one");
                continue;
            }

            var useCase = handled[0].GetGenericArguments()[0];

            if (!handler.IsSealed)
            {
                problems.Add($"{handler.Name} is not sealed");
            }

            if (useCase.GetMethod("<Clone>$") is null)
            {
                problems.Add($"{useCase.Name}, handled by {handler.Name}, is not a record");
            }

            if (handler.Name != $"{useCase.Name}Handler" || handler.Namespace != useCase.Namespace)
            {
                problems.Add($"{handler.FullName} should be {useCase.Namespace}.{useCase.Name}Handler");
            }
        }

        Assert.Empty(problems);
    }

    /// <summary>
    /// A command is <c>&lt;UseCase&gt;Command</c> in a <c>Commands</c> namespace, a query <c>&lt;UseCase&gt;Query</c> in
    /// a <c>Queries</c> namespace, so what a use case does shows in its name and where it lives.
    /// </summary>
    [Theory]
    [MemberData(nameof(ApplicationAssemblies))]
    public void EveryUseCase_IsNamedAndFiledAfterItsKind(string assemblyName)
    {
        var problems = new List<string>();
        var handledInterfaces = HandlersIn(RentarAssembly.Parse(assemblyName))
            .SelectMany(UseCaseRegistrationExtensions.HandledInterfacesOf);

        foreach (var handled in handledInterfaces)
        {
            var isQuery = handled.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
            var (suffix, folder) = isQuery ? ("Query", "Queries") : ("Command", "Commands");
            var useCase = handled.GetGenericArguments()[0];

            if (!useCase.Name.EndsWith(suffix, StringComparison.Ordinal))
            {
                problems.Add($"{useCase.Name} is a {suffix.ToLowerInvariant()}, so it should be named {useCase.Name}{suffix}");
            }

            if (useCase.Namespace?.EndsWith($".{folder}", StringComparison.Ordinal) != true)
            {
                problems.Add($"{useCase.FullName} is a {suffix.ToLowerInvariant()}, so it belongs in a {folder} namespace");
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void EveryUseCase_HasExactlyOneHandler()
    {
        var shared = ApplicationLayers()
            .SelectMany(HandlersIn)
            .SelectMany(handler => UseCaseRegistrationExtensions.HandledInterfacesOf(handler)
                .Select(handled => (UseCase: handled.GetGenericArguments()[0], Handler: handler)))
            .GroupBy(pair => pair.UseCase)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.Name}: {string.Join(", ", group.Select(pair => pair.Handler.Name))}");

        Assert.Empty(shared);
    }

    [Fact]
    public void EveryHandler_IsRegisteredByItsModule()
    {
        var missing = ApplicationLayers()
            .SelectMany(HandlersIn)
            .SelectMany(UseCaseRegistrationExtensions.HandledInterfacesOf)
            .Where(handled => !ModuleContainer.Value.IsRegistered(handled))
            .Select(handled => handled.ToString());

        Assert.Empty(missing);
    }

    [Theory]
    [MemberData(nameof(ApplicationAssemblies))]
    public void EveryValidator_IsSealed_AndValidatesAUseCaseNamedAfterIt(string assemblyName)
    {
        var assembly = RentarAssembly.Parse(assemblyName);
        var handledUseCases = HandlersIn(assembly)
            .SelectMany(UseCaseRegistrationExtensions.HandledInterfacesOf)
            .Select(handled => handled.GetGenericArguments()[0])
            .ToHashSet();
        var problems = new List<string>();

        foreach (var validator in ValidatorsIn(assembly))
        {
            var useCase = ValidatedUseCaseOf(validator);
            if (useCase is null)
            {
                problems.Add($"{validator.Name} does not derive from UseCaseValidator<TUseCase>");
                continue;
            }

            if (!validator.IsSealed)
            {
                problems.Add($"{validator.Name} is not sealed");
            }

            if (validator.Name != $"{useCase.Name}Validator" || validator.Namespace != useCase.Namespace)
            {
                problems.Add($"{validator.FullName} should be {useCase.Namespace}.{useCase.Name}Validator");
            }

            if (!handledUseCases.Contains(useCase))
            {
                problems.Add($"{validator.Name} validates {useCase.Name}, which no handler handles, so it never runs");
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void EveryValidator_IsRegisteredByItsModule()
    {
        var missing = ApplicationLayers()
            .SelectMany(ValidatorsIn)
            .Select(ValidatedUseCaseOf)
            .Where(useCase => useCase != null)
            .Select(useCase => typeof(IValidator<>).MakeGenericType(useCase))
            .Where(service => !ModuleContainer.Value.IsRegistered(service))
            .Select(service => service.ToString());

        Assert.Empty(missing);
    }

    private static IEnumerable<RentarAssembly> ApplicationLayers()
    {
        return RentarAssembly.All.Where(assembly => assembly is { IsModule: true, Layer: "Application" });
    }

    private static IEnumerable<Type> HandlersIn(RentarAssembly assembly)
    {
        return assembly.Load().GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && UseCaseRegistrationExtensions.HandledInterfacesOf(type).Any());
    }

    private static IEnumerable<Type> ValidatorsIn(RentarAssembly assembly)
    {
        return assembly.Load().GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IValidator).IsAssignableFrom(type));
    }

    /// <summary>
    /// The <c>TUseCase</c> of the <see cref="UseCaseValidator{TUseCase}"/> the validator derives from, or <c>null</c>.
    /// </summary>
    private static Type ValidatedUseCaseOf(Type validator)
    {
        for (var type = validator.BaseType; type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UseCaseValidator<>))
            {
                return type.GetGenericArguments()[0];
            }
        }

        return null;
    }

    /// <summary>Registers every module's container setup the way the host does. Nothing is resolved.</summary>
    private static IContainer BuildModuleContainer()
    {
        var builder = new ContainerBuilder();

        var modules = RentarAssembly.All
            .Where(assembly => assembly is { IsModule: true, Layer: "Infrastructure" })
            .SelectMany(assembly => assembly.Load().GetTypes())
            .Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract)
            .Select(type => (IModule)Activator.CreateInstance(type)!);

        foreach (var module in modules)
        {
            module.ConfigureContainer(builder);
        }

        return builder.Build();
    }
}
