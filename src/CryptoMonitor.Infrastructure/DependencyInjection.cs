using CryptoMonitor.Application.Options;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.BackgroundServices;
using CryptoMonitor.Infrastructure.CoinCap;
using CryptoMonitor.Infrastructure.Persistence;
using CryptoMonitor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();

        services.AddTransient<CoinCapAuthHandler>();

        services.AddHttpClient<ICoinCapApiClient, CoinCapApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CoinCapOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + '/');
        })
        .AddHttpMessageHandler<CoinCapAuthHandler>()
        .AddStandardResilienceHandler();

        services.AddHostedService<CoinCapSyncBackgroundService>();

        return services;
    }
}
