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

    private static async Task<Ok<IReadOnlyList<AlertDto>>> GetAlerts(
        [FromQuery, Description("Time window in hours to detect price variation. Defaults to server-configured value (24h). Must be greater than 0.")] int? windowHours,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var alerts = await mediator.Send(new GetAlertsQuery(windowHours), cancellationToken);
        return TypedResults.Ok(alerts);
    }
}
