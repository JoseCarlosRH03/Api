using System.ComponentModel;
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

    private static async Task<Ok<PagedResult<AssetDto>>> GetAllAssets(
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery, Description("Page number (1-based).")] int page = 1,
        [FromQuery, Description("Items per page. Max 200.")] int pageSize = 50
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var assets = await mediator.Send(new GetAssetsQuery(page, pageSize), cancellationToken);
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

    private static async Task<Ok<PagedResult<PriceHistoryDto>>> GetAssetHistory(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery, Description("Start date in UTC. Example: 2026-05-01T00:00:00")] DateTime? from = null,
        [FromQuery, Description("End date in UTC. Example: 2026-05-30T23:59:59")] DateTime? to = null,
        [FromQuery, Description("Page number (1-based).")] int page = 1,
        [FromQuery, Description("Items per page. Max 500.")] int pageSize = 100
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 100 : Math.Min(pageSize, 500);
        var history = await mediator.Send(new GetAssetHistoryQuery(id, from, to, page, pageSize), cancellationToken);
        return TypedResults.Ok(history);
    }
}
