using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using FluentValidation;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.Interceptors;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rentar.Shared.Infrastructure.Modules;

public static class UseCaseRegistrationExtensions
{
    private static readonly ProxyGenerationOptions ProxyOptions = new()
    {
        Selector = new AspectInterceptorSelector()
    };

    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    /// <summary>
    /// Registers every use case handler in a module's Application assembly behind an interface proxy, so the aspect
    /// attributes on its <c>Handle</c> method (caching, transactions, ...) run around each call. Also registers the
    /// assembly's validators, which <see cref="RegisterUseCaseValidation"/> runs before the handler, and its integration
    /// event handlers, the module's reactions to other modules' events.
    /// </summary>
    public static void RegisterUseCases(this ContainerBuilder builder, Assembly applicationAssembly)
    {
        builder.RegisterAssemblyTypes(applicationAssembly)
            .Where(type => HandledInterfacesOf(type).Any())
            .As(HandledInterfacesOf)
            .EnableInterfaceInterceptors(ProxyOptions)
            .SingleInstance();

        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(IValidator<>))
            .SingleInstance();

        // Event handlers only adapt an event to one of the module's use cases, whose own aspects run as usual.
        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .SingleInstance();
    }

    /// <summary>
    /// Wraps every handler so its use case's validators run first. The wrapper sits outside the aspect proxy, so
    /// invalid input never opens a transaction or clears a cache. Call it once for the whole container: each call
    /// adds one more wrapper around every handler of every module. Also registers the validator every paged query
    /// shares; Autofac only offers it for queries that satisfy its <see cref="Rentar.Shared.Application.Data.IPagedQuery"/> constraint.
    /// </summary>
    public static void RegisterUseCaseValidation(this ContainerBuilder builder)
    {
        builder.RegisterGenericDecorator(typeof(ValidatingCommandHandler<>), typeof(ICommandHandler<>));
        builder.RegisterGenericDecorator(typeof(ValidatingCommandHandler<,>), typeof(ICommandHandler<,>));
        builder.RegisterGenericDecorator(typeof(ValidatingQueryHandler<,>), typeof(IQueryHandler<,>));

        builder.RegisterGeneric(typeof(PagedQueryValidator<>))
            .As(typeof(IValidator<>))
            .SingleInstance();
    }

    /// <summary>
    /// The closed handler interfaces a type implements, e.g. <c>IQueryHandler&lt;GetBrandsQuery, List&lt;Brand&gt;&gt;</c>.
    /// </summary>
    public static IEnumerable<Type> HandledInterfacesOf(Type type)
    {
        return type.GetInterfaces()
            .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));
    }
}
