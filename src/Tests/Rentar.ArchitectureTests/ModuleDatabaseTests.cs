using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Modules;
using Rentar.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rentar.ArchitectureTests;

/// <summary>
/// Each module owns one context, registered onto a schema named after the module, which also holds the module's outbox
/// and inbox. Checked against the modules' real registrations; no database is contacted.
/// </summary>
public class ModuleDatabaseTests
{
    /// <summary>Every module: even one without data of its own (Payments) keeps the events it publishes and handles.</summary>
    private static readonly string[] ModulesWithDatabase = ["Companies", "Customers", "Findeks", "Fleet", "Identity", "Payments", "Rentals"];

    private static readonly Type[] MessagingEntities = [typeof(OutboxMessage), typeof(InboxMessage)];

    private static readonly Lazy<IServiceProvider> ModuleServices = new(BuildModuleServices);

    public static TheoryData<Type> Contexts()
    {
        return new TheoryData<Type>(ContextTypes());
    }

    [Fact]
    public void ModulesWithState_HaveExactlyOneContextEach_AndStatelessModulesNone()
    {
        var modules = ContextTypes().Select(ModuleOf).ToList();

        Assert.Equal(ModulesWithDatabase, modules.Order());
    }

    [Fact]
    public void ModuleDatabases_AreRegisteredOnePerModule_WithTheirOwnSchema()
    {
        var schemas = ModuleServices.Value.GetServices<IModuleDatabase>().Select(database => database.Schema);

        Assert.Equal(ModulesWithDatabase.Select(module => module.ToLowerInvariant()), schemas.Order());
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Context_MapsOnlyItsOwnDomain_IntoTheModuleSchema(Type contextType)
    {
        var module = ModuleOf(contextType);
        using var context = CreateRegisteredContext(contextType);

        Assert.NotEmpty(context.Model.GetEntityTypes());
        Assert.All(context.Model.GetEntityTypes(), entityType =>
        {
            Assert.Equal(module.ToLowerInvariant(), entityType.GetSchema());
            if (!MessagingEntities.Contains(entityType.ClrType))
            {
                Assert.Equal($"Rentar.Modules.{module}.Domain", entityType.ClrType.Assembly.GetName().Name);
            }
        });
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Context_MapsTheModulesOutboxAndInbox(Type contextType)
    {
        using var context = CreateRegisteredContext(contextType);

        Assert.All(MessagingEntities, entity => Assert.NotNull(context.Model.FindEntityType(entity)));
    }

    /// <summary>
    /// A module's events go to its own outbox and its handlers record into its own inbox, so each module registers one
    /// message store, over its own context, for its own Contracts and Application.
    /// </summary>
    [Fact]
    public void MessageStores_AreRegisteredOnePerModule_ForItsSchemaContractsAndApplication()
    {
        var stores = ModuleServices.Value.GetServices<IModuleMessageStore>().OrderBy(store => store.Schema).ToList();

        Assert.Equal(ModulesWithDatabase.Select(module => module.ToLowerInvariant()), stores.Select(store => store.Schema));
        Assert.All(stores, store =>
        {
            var module = ModulesWithDatabase.Single(m => m.ToLowerInvariant() == store.Schema);
            Assert.Equal($"Rentar.Modules.{module}.Contracts", store.ContractsAssembly.GetName().Name);
            Assert.Equal($"Rentar.Modules.{module}.Application", store.ApplicationAssembly.GetName().Name);
        });
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Context_KeepsItsMigrationHistoryInTheModuleSchema(Type contextType)
    {
        using var context = CreateRegisteredContext(contextType);

        var options = context.GetService<IDbContextOptions>();
        Assert.Equal(ModuleOf(contextType).ToLowerInvariant(), RelationalOptionsExtension.Extract(options).MigrationsHistoryTableSchema);
    }

    private static IEnumerable<Type> ContextTypes()
    {
        return RentarAssembly.All
            .Where(assembly => assembly.IsModule)
            .SelectMany(assembly => assembly.Load().GetTypes())
            .Where(type => typeof(DbContext).IsAssignableFrom(type) && !type.IsAbstract);
    }

    private static string ModuleOf(Type type)
    {
        return RentarAssembly.Parse(type.Assembly.GetName().Name!).Module;
    }

    /// <summary>Registers every module the way the host does, minus the Autofac container.</summary>
    private static IServiceProvider BuildModuleServices()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        var modules = RentarAssembly.All
            .Where(assembly => assembly is { IsModule: true, Layer: "Infrastructure" })
            .SelectMany(assembly => assembly.Load().GetTypes())
            .Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract)
            .Select(type => (IModule)Activator.CreateInstance(type)!);

        foreach (var module in modules)
        {
            module.AddServices(services, configuration);
        }

        return services.BuildServiceProvider();
    }

    private static DbContext CreateRegisteredContext(Type contextType)
    {
        var factoryType = typeof(IDbContextFactory<>).MakeGenericType(contextType);
        var factory = ModuleServices.Value.GetRequiredService(factoryType);

        return (DbContext)factoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))!.Invoke(factory, null)!;
    }
}
