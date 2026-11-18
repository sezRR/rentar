using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rentar.Shared.Presentation.Endpoints;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Rentar.Shared.Presentation.Maintenance;

/// <summary>
/// Refuses versioned API requests during maintenance before authentication, binding or use cases run.
/// Non-API endpoints, static files and background event delivery are not affected.
/// </summary>
public sealed class MaintenanceMiddleware(RequestDelegate next, IOptionsMonitor<MaintenanceOptions> options, TimeProvider timeProvider)
{
    private const string Message = "Servers in Maintenance (Time)";
    private static readonly ApiVersion V1 = new(1, 0);

    public async Task InvokeAsync(HttpContext context)
    {
        var version = context.GetEndpoint()?.Metadata.GetMetadata<ApiVersionMetadata>();
        var settings = options.CurrentValue;
        var now = timeProvider.GetLocalNow();

        if (version is null || !settings.Enabled || !TimeOnly.FromDateTime(now.DateTime).IsBetween(settings.StartTime, settings.EndTime))
        {
            await next(context);
            return;
        }

        var remaining = settings.EndTime.ToTimeSpan() - now.TimeOfDay;
        if (remaining <= TimeSpan.Zero)
        {
            remaining += TimeSpan.FromDays(1);
        }

        context.Response.Headers.RetryAfter = Math.Ceiling(remaining.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        context.Response.Headers.CacheControl = "no-store";

        IResult response = version.IsMappedTo(V1)
            ? TypedResults.Json(new V1Response<object>(null, false, Message), statusCode: StatusCodes.Status503ServiceUnavailable)
            : TypedResults.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: Message);

        await response.ExecuteAsync(context);
    }
}
