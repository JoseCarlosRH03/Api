using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Exceptions;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Queries;

public sealed record GetAssetHistoryQuery(
    string AssetId,
    DateTimeOffset? From,
    DateTimeOffset? To) : IRequest<IReadOnlyList<PriceHistoryDto>>;

internal sealed class GetAssetHistoryQueryHandler(
    IAssetRepository assetRepository,
    IPriceHistoryRepository historyRepository)
    : IRequestHandler<GetAssetHistoryQuery, IReadOnlyList<PriceHistoryDto>>
{
    public async Task<IReadOnlyList<PriceHistoryDto>> Handle(GetAssetHistoryQuery request, CancellationToken cancellationToken)
    {
        var exists = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken);
        if (exists is null)
            throw new AssetNotFoundException(request.AssetId);

        var history = await historyRepository.GetByAssetIdAsync(
            request.AssetId, request.From, request.To, cancellationToken);

        return history.Select(PriceHistoryDto.From).ToList();
    }
}
