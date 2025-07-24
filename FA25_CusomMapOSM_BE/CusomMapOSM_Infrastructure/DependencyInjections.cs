using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis;
using System.Net.Sockets;

namespace CusomMapOSM_Infrastructure;

public static class DependencyInjections
{
    private const int RETRY_ATTEMPTS = 3;
    private const int RETRY_DELAY = 10;

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistance(configuration);
        services.AddServices(configuration);
        services.AddBackgroundJobs(configuration);
        services.AddPayments(configuration);

        return services;
    }

    public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext for the application
        //services.AddDbContext<DiamondShopDbContext>(opt =>
        //{
        //    opt.UseMySql(configuration.GetConnectionString("DefaultConnection"),
        //        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")));
        //});

        // Register Repositories


        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add application services

        // Register Redis Cache
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var host = Environment.GetEnvironmentVariable("REDIS_HOST");
            var port = Environment.GetEnvironmentVariable("REDIS_PORT");
            var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
            var redisConnectionString = $"{host}:{port},password={password}";

            var policy = Policy
                .Handle<RedisConnectionException>()
                .Or<SocketException>()
                .WaitAndRetry(RETRY_ATTEMPTS, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            return policy.Execute(() => ConnectionMultiplexer.Connect(redisConnectionString));
        });
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        // File service persist
        //services.AddSingleton((serviceProvider) =>
        //{
        //    IOptions<ExternalUrlsOptions> getOption = serviceProvider.GetRequiredService<IOptions<ExternalUrlsOptions>>();
        //    if (getOption is null)
        //        throw new ArgumentNullException();
        //    ExternalUrlsOptions option = getOption.Value;
        //    var newClient = new BlobServiceClient(option.Azure.ConnectionString, new BlobClientOptions() { });
        //    return newClient;
        //});
        //services.AddScoped<IBlobFileServices, AzureBlobContainerService>();

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Add background job services

        return services;
    }

    internal static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration)
    {
        // Add payment services

        return services;
    }
}
