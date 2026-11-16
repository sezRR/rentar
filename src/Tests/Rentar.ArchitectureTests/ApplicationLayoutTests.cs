using System;
using System.Linq;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// What a module's use cases share sits under its Application project's <c>Common</c> folder: the interfaces
/// Infrastructure implements in <c>Common/Interfaces</c>, the models queries answer with in <c>Common/Models</c>.
/// </summary>
public class ApplicationLayoutTests
{
    [Theory]
    [MemberData(nameof(UseCaseTests.ApplicationAssemblies), MemberType = typeof(UseCaseTests))]
    public void EveryInterface_LivesInCommonInterfaces(string assemblyName)
    {
        var expected = $"{assemblyName}.Common.Interfaces";

        var misplaced = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.IsInterface && type.Namespace != expected)
            .Select(type => $"{type.FullName} should be in {expected}");

        Assert.Empty(misplaced);
    }

    [Theory]
    [MemberData(nameof(UseCaseTests.ApplicationAssemblies), MemberType = typeof(UseCaseTests))]
    public void NoType_IsADto(string assemblyName)
    {
        var dtos = RentarAssembly.Parse(assemblyName).Load().GetTypes()
            .Where(type => type.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase))
            .Select(type => $"{type.FullName} should be a <Name>Response in Common.Models");

        Assert.Empty(dtos);
    }
}
