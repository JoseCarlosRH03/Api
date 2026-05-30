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
        [FromQuery, Description("Page number (1-based). Default: 1.")] int page,
        [FromQuery, Description("Items per page. Default: 50.")] int pageSize,
        IMediator mediator,
        CancellationToken cancellationToken
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
        [FromQuery, Description("Start date in UTC. Example: 2026-05-01T00:00:00")] DateTime? from,
        [FromQuery, Description("End date in UTC. Example: 2026-05-30T23:59:59")] DateTime? to,
        [FromQuery, Description("Page number (1-based). Default: 1.")] int page,
        [FromQuery, Description("Items per page. Default: 100.")] int pageSize,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 100 : Math.Min(pageSize, 500);
        var history = await mediator.Send(new GetAssetHistoryQuery(id, from, to, page, pageSize), cancellationToken);
        return TypedResults.Ok(history);
    }
}
