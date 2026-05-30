using CryptoMonitor.Application.Interfaces;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.Configure<PriceAlertOptions>(configuration.GetSection("PriceAlert"));
        services.Configure<CoinCapOptions>(configuration.GetSection("CoinCap"));

        services.AddScoped<IAlertDetectionService, AlertDetectionService>();

        return services;
    }
}
