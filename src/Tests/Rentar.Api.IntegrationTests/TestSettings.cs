using Microsoft.Extensions.Configuration;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// Configuration a test changes while the shared host runs, e.g. to open a maintenance window or to enable seeding,
/// without building a host of its own. It is the last configuration source, so its values win over every other one.
/// </summary>
public sealed class TestSettings : ConfigurationProvider, IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return this;
    }

    /// <summary>
    /// Sets every value, then reloads once. Options rebind and validate on each reload, so setting related values one
    /// by one could pass through a combination that is invalid, such as a maintenance window starting where it ends.
    /// </summary>
    public void Apply(params (string Key, string Value)[] settings)
    {
        foreach (var (key, value) in settings)
        {
            Data[key] = value;
        }

        OnReload();
    }

    /// <summary>Drops every value a test applied, so the configuration is as the factory set it up.</summary>
    public void Reset()
    {
        Data.Clear();
        OnReload();
    }
}
