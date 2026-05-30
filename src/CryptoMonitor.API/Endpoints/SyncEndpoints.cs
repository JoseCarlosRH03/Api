using CryptoMonitor.Application.Assets.Commands;
using CryptoMonitor.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CryptoMonitor.API.Endpoints;

public static class SyncEndpoints
{
    public static RouteGroupBuilder MapSyncEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", TriggerSync);
        return group;
    }

    private static async Task<Accepted<SyncResultDto>> TriggerSync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SyncAssetsCommand(), cancellationToken);
        return TypedResults.Accepted((string?)null, result);
    }
}
