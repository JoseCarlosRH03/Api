using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CryptoMonitor.IntegrationTests.Infrastructure;

public sealed class GetLastSyncTimeTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        factory.ResetDatabase();
        factory.CoinCapClientMock.Reset();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLastSyncTimeAsync_WhenNoRecords_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();

        var result = await repository.GetLastSyncTimeAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLastSyncTimeAsync_WhenRecordsExist_ReturnsMaxRecordedAt()
    {
        var older = DateTime.UtcNow.AddMinutes(-10);
        var newer = DateTime.UtcNow.AddMinutes(-2);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Assets.Add(new Asset { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow });
            db.PriceHistories.AddRange(
                new PriceHistory { AssetId = "bitcoin", PriceUsd = 49000m, RecordedAt = older },
                new PriceHistory { AssetId = "bitcoin", PriceUsd = 50000m, RecordedAt = newer });
            await db.SaveChangesAsync();
        }

        using var queryScope = factory.Services.CreateScope();
        var repository = queryScope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();

        var result = await repository.GetLastSyncTimeAsync();

        result.Should().NotBeNull();
        result!.Value.Should().BeCloseTo(newer, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLastSyncTimeAsync_AfterSync_ReturnsTimestampOfLastSync()
    {
        factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }]);

        var beforeSync = DateTime.UtcNow;
        var client = factory.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);
        var afterSync = DateTime.UtcNow;

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();

        var result = await repository.GetLastSyncTimeAsync();

        result.Should().NotBeNull();
        result!.Value.Should().BeOnOrAfter(beforeSync).And.BeOnOrBefore(afterSync);
    }
}
