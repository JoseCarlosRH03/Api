using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.E2ETests;

public sealed class FullFlowTests(E2ETestFixture fixture)
    : IClassFixture<E2ETestFixture>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        fixture.ResetDatabase();
        fixture.CoinCapClientMock.Reset();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task FullFlow_SyncThenListDetailAndHistory_AllReturn200()
    {
        fixture.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset>
            {
                new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }
            });
        var client = fixture.CreateApiClient();

        var syncResponse = await client.PostAsync("/api/v1/sync", null);
        syncResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var listResponse = await client.GetAsync("/api/v1/assets");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assets = await listResponse.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        assets!.Items.Should().HaveCount(1);

        var detailResponse = await client.GetAsync("/api/v1/assets/bitcoin");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var asset = await detailResponse.Content.ReadFromJsonAsync<AssetDto>();
        asset!.Symbol.Should().Be("BTC");

        var historyResponse = await client.GetAsync("/api/v1/assets/bitcoin/history");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyResponse.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        history!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task MultipleSyncs_DoNotDuplicateAssets_UpsertIsCorrect()
    {
        fixture.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = fixture.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);

        var response = await client.GetAsync("/api/v1/assets");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        result!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HistoryWithFutureFromFilter_ReturnsEmpty()
    {
        fixture.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = fixture.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var futureDate = DateTime.UtcNow.AddDays(1).ToString("s");
        var response = await client.GetAsync($"/api/v1/assets/bitcoin/history?from={futureDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        history!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HistoryWithPastFromFilter_ReturnsRecords()
    {
        fixture.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = fixture.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var pastDate = DateTime.UtcNow.AddDays(-1).ToString("s");
        var response = await client.GetAsync($"/api/v1/assets/bitcoin/history?from={pastDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        history!.Items.Should().HaveCount(1);
    }
}
