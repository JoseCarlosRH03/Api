using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Exceptions;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Queries;

public sealed record GetAssetHistoryQuery(
    string AssetId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 100) : IRequest<PagedResult<PriceHistoryDto>>;

internal sealed class GetAssetHistoryQueryHandler(
    IAssetRepository assetRepository,
    IPriceHistoryRepository historyRepository)
    : IRequestHandler<GetAssetHistoryQuery, PagedResult<PriceHistoryDto>>
{
    public async Task<PagedResult<PriceHistoryDto>> Handle(GetAssetHistoryQuery request, CancellationToken cancellationToken)
    {
        var exists = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken);
        if (exists is null)
            throw new AssetNotFoundException(request.AssetId);

        var (items, totalCount) = await historyRepository.GetPagedByAssetIdAsync(
            request.AssetId, request.From, request.To, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<PriceHistoryDto>(
            items.Select(PriceHistoryDto.From).ToList(), totalCount, request.Page, request.PageSize);
    }
}
