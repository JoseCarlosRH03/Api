using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CryptoMonitor.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "integration-test-key";
    public Mock<ICoinCapApiClient> CoinCapClientMock { get; } = new();

    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"crypto_test_{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSecurity:ApiKey"] = TestApiKey,
                ["CoinCap:ApiKey"] = "test-key",
                ["CoinCap:BaseUrl"] = "https://api.coincap.io/v2",
                ["PriceAlert:ThresholdPercent"] = "5.0",
                ["PriceAlert:WindowHours"] = "24",
                ["PriceAlert:RetentionDays"] = "30",
                ["ConnectionStrings:Default"] = $"DataSource={_dbPath}"
            });
        });

        builder.ConfigureServices(services =>
        {
            var coinCapDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICoinCapApiClient));
            if (coinCapDescriptor != null) services.Remove(coinCapDescriptor);
            services.AddSingleton<ICoinCapApiClient>(_ => CoinCapClientMock.Object);

            services.RemoveAll(typeof(IHostedService));
        });
    }

    public HttpClient CreateApiClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
        return client;
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.ExecuteSqlRaw("DELETE FROM PriceHistories");
        db.Database.ExecuteSqlRaw("DELETE FROM Assets");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); }
            catch (IOException) { /* file still held; OS will clean temp dir */ }
        }
    }
}
