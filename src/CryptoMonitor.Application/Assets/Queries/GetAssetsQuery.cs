using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Queries;

public sealed record GetAssetsQuery(int Page = 1, int PageSize = 50) : IRequest<PagedResult<AssetDto>>;

internal sealed class GetAssetsQueryHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetsQuery, PagedResult<AssetDto>>
{
    public async Task<PagedResult<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new PagedResult<AssetDto>(items.Select(AssetDto.From).ToList(), totalCount, request.Page, request.PageSize);
    }
}
