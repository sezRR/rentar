using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Rentar.Shared.Presentation.Maintenance;

public static class MaintenanceExtensions
{
    /// <summary>Binds and validates the maintenance schedule; configuration reloads apply to later requests.</summary>
    public static IServiceCollection AddApiMaintenance(this IServiceCollection services)
    {
        services.AddOptions<MaintenanceOptions>()
            .BindConfiguration(MaintenanceOptions.SectionName)
            .Validate(options => options.StartTime != options.EndTime, "Maintenance start and end times must differ.")
            .ValidateOnStart();
        return services;
    }

    /// <summary>Gates every versioned API endpoint. Place after routing and CORS, before authentication.</summary>
    public static IApplicationBuilder UseApiMaintenance(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MaintenanceMiddleware>();
    }
}
