using CryptoMonitor.Application.Assets.Commands;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Application.Telemetry;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CryptoMonitor.UnitTests.Handlers;

public sealed class SyncAssetsCommandHandlerTests
{
    private readonly Mock<ICoinCapApiClient> _coinCapMock = new();
    private readonly Mock<IAssetRepository> _assetRepoMock = new();
    private readonly Mock<IPriceHistoryRepository> _historyRepoMock = new();
    private readonly Mock<ICryptoMonitorMetrics> _metricsMock = new();

    private SyncAssetsCommandHandler CreateHandler(int retentionDays = 30) =>
        new(_coinCapMock.Object, _assetRepoMock.Object, _historyRepoMock.Object,
            Options.Create(new PriceAlertOptions { RetentionDays = retentionDays }),
            _metricsMock.Object);

    [Fact]
    public async Task SyncAssetsCommandHandler_WhenApiSucceeds_UpsertAndRecordsHistory()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow },
            new() { Id = "ethereum", Symbol = "ETH", Name = "Ethereum", PriceUsd = 3000m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        _coinCapMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assets);

        var result = await CreateHandler().Handle(new SyncAssetsCommand(), CancellationToken.None);

        result.AssetsSynced.Should().Be(2);
        _assetRepoMock.Verify(r => r.UpsertRangeAsync(assets, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<PriceHistory>>(), It.IsAny<CancellationToken>()), Times.Once);
        _historyRepoMock.Verify(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _metricsMock.Verify(m => m.RecordSync(2, It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task SyncAssetsCommandHandler_WhenRetentionIsZero_DoesNotDeleteHistory()
    {
        _coinCapMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }]);

        await CreateHandler(retentionDays: 0).Handle(new SyncAssetsCommand(), CancellationToken.None);

        _historyRepoMock.Verify(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncAssetsCommandHandler_WhenApiFails_RecordsSyncErrorAndRethrows()
    {
        _coinCapMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var act = async () => await CreateHandler().Handle(new SyncAssetsCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
        _assetRepoMock.Verify(r => r.UpsertRangeAsync(It.IsAny<IEnumerable<Asset>>(), It.IsAny<CancellationToken>()), Times.Never);
        _historyRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<PriceHistory>>(), It.IsAny<CancellationToken>()), Times.Never);
        _metricsMock.Verify(m => m.RecordSyncError(), Times.Once);
    }
}
