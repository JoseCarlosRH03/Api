using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.IntegrationTests.Alerts;

public sealed class GetAlertsTests(CustomWebApplicationFactory factory)
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
    public async Task GetAlerts_WhenNoHistory_ReturnsEmptyList()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAlerts_WhenVariationExceedsThreshold_ReturnsAlert()
    {
        factory.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset>
            {
                new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }
            })
            .ReturnsAsync(new List<Asset>
            {
                new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 106m, UpdatedAt = DateTimeOffset.UtcNow }
            });
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().HaveCount(1);
        alerts![0].AssetId.Should().Be("bitcoin");
        alerts[0].VariationPercent.Should().BeApproximately(6.0, 0.01);
    }

    [Fact]
    public async Task GetAlerts_WhenVariationBelowThreshold_ReturnsNoAlert()
    {
        factory.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }])
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 103m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAlerts_WhenPriceDropExceedsThreshold_ReturnsAlert()
    {
        factory.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }])
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 90m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().HaveCount(1);
        alerts![0].VariationPercent.Should().BeApproximately(-10.0, 0.01);
    }

    [Fact]
    public async Task GetAlerts_WithExplicitWindowHours_UsesProvidedWindow()
    {
        factory.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }])
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 110m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/alerts?windowHours=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>();
        alerts.Should().HaveCount(1);
        alerts![0].VariationPercent.Should().BeApproximately(10.0, 0.01);
    }

    [Fact]
    public async Task GetAlerts_WithZeroWindowHours_Returns400ValidationProblem()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/alerts?windowHours=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetAlerts_WithNegativeWindowHours_Returns400ValidationProblem()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/alerts?windowHours=-5");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
