using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CryptoMonitor.IntegrationTests.Assets;

public sealed class GetAssetHistoryTests(CustomWebApplicationFactory factory)
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
    public async Task GetAssetHistory_AfterMultipleSyncs_ReturnsAllRecords()
    {
        factory.CoinCapClientMock
            .SetupSequence(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }])
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 110m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/assets/bitcoin/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAssetHistory_WithFutureFromFilter_ReturnsEmpty()
    {
        factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var futureDate = DateTime.UtcNow.AddDays(1).ToString("s");
        var response = await client.GetAsync($"/api/v1/assets/bitcoin/history?from={futureDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAssetHistory_WithPastFromFilter_ReturnsAllRecords()
    {
        factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var pastDate = DateTime.UtcNow.AddDays(-1).ToString("s");
        var response = await client.GetAsync($"/api/v1/assets/bitcoin/history?from={pastDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        result!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAssetHistory_WithPastToFilter_ReturnsEmpty()
    {
        factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 100m, UpdatedAt = DateTimeOffset.UtcNow }]);
        var client = factory.CreateApiClient();
        await client.PostAsync("/api/v1/sync", null);

        var pastDate = DateTime.UtcNow.AddDays(-1).ToString("s");
        var response = await client.GetAsync($"/api/v1/assets/bitcoin/history?to={pastDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAssetHistory_WithDateOnlyToFilter_IncludesEntireDay()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Assets.Add(new Asset
        {
            Id = "bitcoin",
            Symbol = "BTC",
            Name = "Bitcoin",
            PriceUsd = 100m,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        db.PriceHistories.Add(new PriceHistory
        {
            AssetId = "bitcoin",
            PriceUsd = 100m,
            RecordedAt = new DateTime(2026, 5, 30, 12, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/bitcoin/history?to=2026-05-30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PriceHistoryDto>>();
        result!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAssetHistory_ForNonExistentAsset_Returns404WithProblemDetails()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/nonexistent/history");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetAssetHistory_WithInvalidPage_Returns400ValidationProblem()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/bitcoin/history?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
