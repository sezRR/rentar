using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// Entities are set once, when they are built: a use case that changes one builds a new one rather than editing
/// what it loaded.
/// </summary>
public class DomainTests
{
    public static TheoryData<string> DomainAssemblies()
    {
        return new TheoryData<string>(RentarAssembly.All
            .Where(assembly => assembly is { IsModule: true, Layer: "Domain" })
            .Select(assembly => assembly.Name));
    }

    [Theory]
    [MemberData(nameof(DomainAssemblies))]
    public void EveryEntityProperty_IsInitOnly(string assemblyName)
    {
        var settable = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(property => property.SetMethod is { IsPublic: true } && !IsInitOnly(property))
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name} has a public set; make it init");

        Assert.Empty(settable);
    }

    private static bool IsInitOnly(PropertyInfo property)
    {
        return property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));
    }
}
