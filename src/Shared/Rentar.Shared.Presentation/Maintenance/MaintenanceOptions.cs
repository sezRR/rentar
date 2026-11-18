using System;

namespace Rentar.Shared.Presentation.Maintenance;

/// <summary>A daily maintenance window in server-local time, including windows that cross midnight.</summary>
public sealed class MaintenanceOptions
{
    public const string SectionName = "Maintenance";

    public bool Enabled { get; set; }

    public TimeOnly StartTime { get; set; } = new(22, 0);

    public TimeOnly EndTime { get; set; } = new(23, 0);
}
