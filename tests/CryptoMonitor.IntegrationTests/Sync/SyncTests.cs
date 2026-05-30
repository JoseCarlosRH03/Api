using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.IntegrationTests.Sync;

public sealed class SyncTests(CustomWebApplicationFactory factory)
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
    public async Task Sync_WhenApiSucceeds_Returns202AndPersistsAssets()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        factory.CoinCapClientMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assets);
        var client = factory.CreateApiClient();

        var syncResponse = await client.PostAsync("/api/v1/sync", null);

        syncResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await syncResponse.Content.ReadFromJsonAsync<SyncResultDto>();
        result!.AssetsSynced.Should().Be(1);

        var assetsResponse = await client.GetAsync("/api/v1/assets");
        var persisted = await assetsResponse.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        persisted!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Sync_WhenApiFails_Returns502WithProblemDetails()
    {
        factory.CoinCapClientMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));
        var client = factory.CreateApiClient();

        var response = await client.PostAsync("/api/v1/sync", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
