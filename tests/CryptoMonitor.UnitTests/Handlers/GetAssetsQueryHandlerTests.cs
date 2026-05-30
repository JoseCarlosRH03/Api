using CryptoMonitor.Application.Assets.Queries;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.UnitTests.Handlers;

public sealed class GetAssetsQueryHandlerTests
{
    private readonly Mock<IAssetRepository> _repoMock = new();

    [Fact]
    public async Task GetAssetsQueryHandler_ReturnsPagedAssets()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow },
            new() { Id = "ethereum", Symbol = "ETH", Name = "Ethereum", PriceUsd = 3000m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        _repoMock.Setup(r => r.GetPagedAsync(1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Asset>)assets, assets.Count));

        var handler = new GetAssetsQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.Items[0].Id.Should().Be("bitcoin");
        result.Items[1].Id.Should().Be("ethereum");
    }
}
