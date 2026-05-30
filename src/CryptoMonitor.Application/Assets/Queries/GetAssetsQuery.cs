using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Queries;

public sealed record GetAssetsQuery : IRequest<IReadOnlyList<AssetDto>>;

internal sealed class GetAssetsQueryHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetsQuery, IReadOnlyList<AssetDto>>
{
    public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await repository.GetAllAsync(cancellationToken);
        return assets.Select(AssetDto.From).ToList();
    }
}
