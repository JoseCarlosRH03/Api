using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Exceptions;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Queries;

public sealed record GetAssetByIdQuery(string Id) : IRequest<AssetDto>;

internal sealed class GetAssetByIdQueryHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetByIdQuery, AssetDto>
{
    public async Task<AssetDto> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AssetNotFoundException(request.Id);

        return AssetDto.From(asset);
    }
}
