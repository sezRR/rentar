using System;
using System.Collections.Generic;

namespace Rentar.Shared.Infrastructure.Persistence.Seeding;

/// <summary>
/// Passes ids between module seeders without exposing their entities or contexts. Keys are qualified by module.
/// A fresh context is built on every run, including when the records already exist.
/// </summary>
public sealed class DevelopmentSeedContext
{
    private readonly Dictionary<string, int> _ids = new(StringComparer.Ordinal);

    public void SetId(string key, int id) => _ids.Add(key, id);

    public int GetId(string key) => _ids.TryGetValue(key, out var id)
        ? id
        : throw new InvalidOperationException($"Development seed '{key}' has not been provided by an earlier module.");
}
