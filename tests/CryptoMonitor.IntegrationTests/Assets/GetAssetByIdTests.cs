using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.IntegrationTests.Assets;

public sealed class GetAssetByIdTests(CustomWebApplicationFactory factory)
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
    public async Task GetAssetById_WhenNotFound_Returns404WithProblemDetails()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetAssetById_WhenExists_ReturnsAsset()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        factory.CoinCapClientMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assets);
        var client = factory.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var response = await client.GetAsync("/api/v1/assets/bitcoin");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asset = await response.Content.ReadFromJsonAsync<AssetDto>();
        asset!.Id.Should().Be("bitcoin");
        asset.Symbol.Should().Be("BTC");
    }
}
