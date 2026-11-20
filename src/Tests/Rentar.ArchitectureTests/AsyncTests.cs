using Rentar.Shared.Application.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// Whatever can reach the database or another module is asynchronous and can be cancelled, so a request never blocks
/// a thread while it waits and stops working once its caller has gone. Use case handlers get this from their
/// interfaces; repositories and module APIs are checked here.
/// </summary>
public class AsyncTests
{
    public static TheoryData<string> ContractsAssemblies()
    {
        return new TheoryData<string>(RentarAssembly.All
            .Where(assembly => assembly is { IsModule: true, Layer: "Contracts" })
            .Select(assembly => assembly.Name));
    }

    [Theory]
    [MemberData(nameof(UseCaseTests.ApplicationAssemblies), MemberType = typeof(UseCaseTests))]
    public void EveryRepositoryMethod_ReturnsATask_AndTakesACancellationToken(string assemblyName)
    {
        var repositories = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.IsInterface && type.GetInterfaces().Any(IsEntityRepository));

        Assert.Empty(Synchronous(repositories));
    }

    [Theory]
    [MemberData(nameof(ContractsAssemblies))]
    public void EveryModuleApiMethod_ReturnsATask_AndTakesACancellationToken(string assemblyName)
    {
        var moduleApis = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("ModuleApi", StringComparison.Ordinal));

        Assert.Empty(Synchronous(moduleApis));
    }

    private static bool IsEntityRepository(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEntityRepository<>);
    }

    /// <summary>Every method of the interfaces, inherited ones included, that is not async and cancellable.</summary>
    private static IEnumerable<string> Synchronous(IEnumerable<Type> interfaces)
    {
        return interfaces
            .SelectMany(type => type.GetInterfaces().Append(type).SelectMany(declaring => declaring.GetMethods()
                .Select(method => (Interface: type, Method: method))))
            .Where(pair => !IsAsyncAndCancellable(pair.Method))
            .Select(pair => $"{pair.Interface.Name}.{pair.Method.Name} should return a Task and take a CancellationToken last");
    }

    private static bool IsAsyncAndCancellable(MethodInfo method)
    {
        return typeof(Task).IsAssignableFrom(method.ReturnType)
            && method.GetParameters().LastOrDefault()?.ParameterType == typeof(CancellationToken);
    }
}
