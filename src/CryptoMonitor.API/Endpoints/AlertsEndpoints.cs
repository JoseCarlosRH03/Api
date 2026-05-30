using CryptoMonitor.Application.Alerts.Queries;
using CryptoMonitor.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CryptoMonitor.API.Endpoints;

public static class AlertsEndpoints
{
    public static RouteGroupBuilder MapAlertsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAlerts);
        return group;
    }

    private static async Task<Ok<IReadOnlyList<AlertDto>>> GetAlerts(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var alerts = await mediator.Send(new GetAlertsQuery(), cancellationToken);
        return TypedResults.Ok(alerts);
    }
}
