using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.E2ETests;

public sealed class AlertFlowTests(E2ETestFixture fixture)
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
    public async Task Alert_AppearsAfterTwoSyncsWithSignificantPriceChange()
    {
        fixture.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset>
            {
                new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }
            })
            .ReturnsAsync(new List<Asset>
            {
                new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 107m, UpdatedAt = DateTimeOffset.UtcNow }
            });
        var client = fixture.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);

        var response = await client.GetAsync("/api/v1/alerts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().HaveCount(1);
        alerts![0].AssetId.Should().Be("bitcoin");
        alerts[0].VariationPercent.Should().BeApproximately(7.0, 0.01);
    }
}
