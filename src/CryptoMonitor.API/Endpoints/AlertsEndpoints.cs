using System.ComponentModel;
using CryptoMonitor.Application.Alerts.Queries;
using CryptoMonitor.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.API.Endpoints;

public static class AlertsEndpoints
{
    public static RouteGroupBuilder MapAlertsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAlerts);
        return group;
    }

    private static async Task<Results<Ok<IReadOnlyList<AlertDto>>, ValidationProblem>> GetAlerts(
        [FromQuery, Description("Time window in hours to detect price variation. Defaults to server-configured value (24h).")] int? windowHours,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (windowHours.HasValue && windowHours.Value <= 0)
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["windowHours"] = ["Must be greater than 0."] });

        var alerts = await mediator.Send(new GetAlertsQuery(windowHours), cancellationToken);
        return TypedResults.Ok(alerts);
    }
}
