using CryptoMonitor.Application.Assets.Queries;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.UnitTests.Handlers;

public sealed class GetAssetHistoryQueryHandlerTests
{
    private readonly Mock<IAssetRepository> _assetRepoMock = new();
    private readonly Mock<IPriceHistoryRepository> _historyRepoMock = new();

    [Fact]
    public async Task GetAssetHistoryQueryHandler_FiltersCorrectlyByDateRange()
    {
        var assetId = "bitcoin";
        var from = DateTime.UtcNow.AddHours(-12);
        var to = DateTime.UtcNow;
        var asset = new Asset { Id = assetId, Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow };
        var history = new List<PriceHistory>
        {
            new() { Id = 1, AssetId = assetId, PriceUsd = 49000m, RecordedAt = from.AddHours(1) },
            new() { Id = 2, AssetId = assetId, PriceUsd = 50000m, RecordedAt = from.AddHours(6) }
        };
        _assetRepoMock.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        _historyRepoMock.Setup(r => r.GetPagedByAssetIdAsync(assetId, from, to, 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<PriceHistory>)history, history.Count));

        var handler = new GetAssetHistoryQueryHandler(_assetRepoMock.Object, _historyRepoMock.Object);
        var result = await handler.Handle(new GetAssetHistoryQuery(assetId, from, to), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].PriceUsd.Should().Be(49000m);
        result.Items[1].PriceUsd.Should().Be(50000m);
        result.TotalCount.Should().Be(2);
    }
}
