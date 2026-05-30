using System.Net;
using FluentAssertions;

namespace CryptoMonitor.IntegrationTests.Security;

public sealed class SecurityTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Request_WithoutApiKey_Returns401WithProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Request_WithWrongApiKey_Returns401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "wrong-key-that-does-not-exist");

        var response = await client.GetAsync("/api/v1/assets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ScalarEndpoint_WithoutApiKey_IsAccessibleInDevelopment()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiEndpoint_WithoutApiKey_IsAccessibleInDevelopment()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MetricsEndpoint_WithoutApiKey_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MetricsEndpoint_WithApiKey_IsAccessible()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
