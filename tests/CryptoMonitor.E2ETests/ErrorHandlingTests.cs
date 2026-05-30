using System.Net;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.E2ETests;

public sealed class ErrorHandlingTests(E2ETestFixture fixture)
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
    public async Task GetAssetById_WhenNotFound_Returns404WithProblemDetails()
    {
        var client = fixture.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/nonexistent-asset");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetAssetHistory_WhenAssetNotFound_Returns404WithProblemDetails()
    {
        var client = fixture.CreateApiClient();

        var response = await client.GetAsync("/api/v1/assets/nonexistent-asset/history");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task AnyEndpoint_WithoutApiKey_Returns401WithProblemDetails()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task AnyEndpoint_WithWrongApiKey_Returns401()
    {
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "wrong-key");

        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
