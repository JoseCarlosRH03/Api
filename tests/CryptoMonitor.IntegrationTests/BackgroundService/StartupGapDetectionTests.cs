using FluentAssertions;
using Moq;

namespace CryptoMonitor.IntegrationTests.BackgroundService;

public sealed class StartupGapDetectionTests : IAsyncLifetime
{
    private readonly StartupGapDetectionFactory _factory = new();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task BackgroundService_WhenGapExceedsInterval_EmitsStartupGapMetric()
    {
        // Arrange: sembrar un registro con RecordedAt 15 min atrás ANTES de que el servidor arranque
        var oldRecordedAt = DateTime.UtcNow.AddMinutes(-15);
        await _factory.SeedOldSyncRecordAsync(oldRecordedAt);

        // El mock devuelve lista vacía para el sync que ocurrirá después del gap detection
        _factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act: crear el cliente fuerza que el servidor arranque y el BackgroundService inicie.
        // DetectStartupGapAsync se ejecuta ANTES del primer Task.Delay(60 min),
        // así que la métrica se emite en los primeros milisegundos del startup.
        var client = _factory.CreateClient();
        await Task.Delay(TimeSpan.FromSeconds(2)); // margen para que el service arranque

        // Assert: GET /metrics devuelve la métrica crypto_monitor_startup_gap
        var metricsResponse = await client.GetStringAsync("/metrics");

        metricsResponse.Should().Contain("crypto_monitor_startup_gap",
            because: "el BackgroundService debe registrar el gap de 15 min detectado al arrancar");
    }

    [Fact]
    public async Task BackgroundService_WhenNoHistory_DoesNotEmitStartupGapMetric()
    {
        // Arrange: DB vacía — primer arranque del servicio, no hay historial previo
        _factory.CoinCapClientMock
            .Setup(c => c.GetAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var client = _factory.CreateClient();
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert: la métrica existe en el output pero con count = 0 (nunca fue emitida)
        var metricsResponse = await client.GetStringAsync("/metrics");

        // Si la métrica fue emitida (gap detectado), count > 0.
        // Con DB vacía, el counter no debe haberse registrado.
        var hasGapObservation = metricsResponse.Contains("crypto_monitor_startup_gap_count");
        if (hasGapObservation)
        {
            // El histograma puede aparecer en /metrics con count=0 si el instrumento
            // fue creado pero nunca llamado con Record(). Esto es correcto.
            var lines = metricsResponse.Split('\n');
            var countLine = lines.FirstOrDefault(l => l.StartsWith("crypto_monitor_startup_gap_count"));
            if (countLine is not null)
                countLine.Should().Contain(" 0", because: "no debería haberse registrado ningún gap en primer arranque");
        }
    }
}
