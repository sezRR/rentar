using Microsoft.Extensions.DependencyInjection;
using Rentar.Shared.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace Rentar.Shared.Presentation.Json;

public static class ApiJsonExtensions
{
    /// <summary>
    /// Writes entities without their navigation properties. Navigations let queries follow relationships; they are
    /// never part of a body, so an entity keeps the shape it has always had, the OpenAPI documents leave them out,
    /// and related entities pointing back at each other can never loop. A response that needs related data uses a
    /// response model.
    /// </summary>
    public static IServiceCollection AddApiJson(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            var resolver = options.SerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
            options.SerializerOptions.TypeInfoResolver = resolver.WithAddedModifier(RemoveNavigations);
        });

        return services;
    }

    private static void RemoveNavigations(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(IEntity).IsAssignableFrom(typeInfo.Type))
        {
            return;
        }

        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            if (IsNavigation(typeInfo.Properties[i].PropertyType))
            {
                typeInfo.Properties.RemoveAt(i);
            }
        }
    }

    /// <summary>A reference to another entity, or a collection of them.</summary>
    private static bool IsNavigation(Type type)
    {
        return typeof(IEntity).IsAssignableFrom(type)
            || type.GetInterfaces().Append(type).Any(candidate =>
                candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                && typeof(IEntity).IsAssignableFrom(candidate.GetGenericArguments()[0]));
    }
}
