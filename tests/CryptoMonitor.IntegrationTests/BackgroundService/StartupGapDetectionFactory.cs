using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CryptoMonitor.IntegrationTests.BackgroundService;

/// <summary>
/// Factory con el BackgroundService habilitado para verificar la detección de gap al inicio.
/// A diferencia de CustomWebApplicationFactory, NO elimina los IHostedService.
/// </summary>
public sealed class StartupGapDetectionFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "gap-detection-test-key";
    public Mock<ICoinCapApiClient> CoinCapClientMock { get; } = new();

    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"crypto_gap_test_{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSecurity:ApiKey"] = TestApiKey,
                ["CoinCap:ApiKey"] = "test-key",
                ["CoinCap:BaseUrl"] = "https://api.coincap.io/v2",
                ["CoinCap:SyncIntervalMinutes"] = "60",   // intervalo largo: el service detecta el gap y luego espera 1h
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
            // IHostedService NO se elimina — el BackgroundService corre de verdad
        });
    }

    /// <summary>
    /// Siembra un registro de PriceHistory con un RecordedAt artificial en el pasado
    /// ANTES de que el servidor arranque (y por ende antes de que el BackgroundService inicie).
    /// </summary>
    public async Task SeedOldSyncRecordAsync(DateTime recordedAt)
    {
        // Crear la DB manualmente ANTES de que el servidor arranque
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource={_dbPath}")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();

        db.Assets.Add(new Asset
        {
            Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin",
            PriceUsd = 50000m, UpdatedAt = DateTimeOffset.UtcNow
        });
        db.PriceHistories.Add(new PriceHistory
        {
            AssetId = "bitcoin", PriceUsd = 50000m, RecordedAt = recordedAt
        });
        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); }
            catch (IOException) { /* OS limpia temp dir */ }
        }
    }
}
