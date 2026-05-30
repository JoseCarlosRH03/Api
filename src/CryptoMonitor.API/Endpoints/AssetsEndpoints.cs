using CryptoMonitor.Application.Assets.Queries;
using CryptoMonitor.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.API.Endpoints;

public static class AssetsEndpoints
{
    public static RouteGroupBuilder MapAssetsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAssets);
        group.MapGet("/{id}", GetAssetById);
        group.MapGet("/{id}/history", GetAssetHistory);
        return group;
    }

    private static async Task<Ok<IReadOnlyList<AssetDto>>> GetAllAssets(
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var assets = await mediator.Send(new GetAssetsQuery(), cancellationToken);
        return TypedResults.Ok(assets);
    }

    private static async Task<Ok<AssetDto>> GetAssetById(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var asset = await mediator.Send(new GetAssetByIdQuery(id), cancellationToken);
        return TypedResults.Ok(asset);
    }

    private static async Task<Ok<IReadOnlyList<PriceHistoryDto>>> GetAssetHistory(
        string id,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var history = await mediator.Send(new GetAssetHistoryQuery(id, from, to), cancellationToken);
        return TypedResults.Ok(history);
    }
}
