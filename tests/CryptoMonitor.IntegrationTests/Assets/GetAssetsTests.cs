using System.Net;
using System.Net.Http.Json;
using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.IntegrationTests.Assets;

public sealed class GetAssetsTests(CustomWebApplicationFactory factory)
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
    public async Task GetAssets_WhenNoData_ReturnsEmptyPagedResult()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        content!.Items.Should().BeEmpty();
        content.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAssets_AfterSync_ReturnsPagedAssets()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        factory.CoinCapClientMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assets);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        content!.Items.Should().HaveCount(1);
        content.TotalCount.Should().Be(1);
        content.Items[0].Id.Should().Be("bitcoin");
    }

    [Fact]
    public async Task GetAssets_WithCustomPageSize_ReturnsCorrectPage()
    {
        factory.CoinCapClientMock.Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new() { Id = "bitcoin",  Symbol = "BTC", Name = "Bitcoin",  PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow },
                new() { Id = "ethereum", Symbol = "ETH", Name = "Ethereum", PriceUsd = 3000m,  UpdatedAt = DateTimeOffset.UtcNow },
                new() { Id = "litecoin", Symbol = "LTC", Name = "Litecoin", PriceUsd = 100m,   UpdatedAt = DateTimeOffset.UtcNow }
            ]);
        var client = factory.CreateApiClient();

        await client.PostAsync("/api/v1/sync", null);
        var response = await client.GetAsync("/api/v1/assets?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<AssetDto>>();
        content!.Items.Should().HaveCount(2);
        content.TotalCount.Should().Be(3);
        content.TotalPages.Should().Be(2);
        content.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAssets_WithInvalidPage_Returns400ValidationProblem()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetAssets_WithTooLargePageSize_Returns400ValidationProblem()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets?pageSize=201");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
