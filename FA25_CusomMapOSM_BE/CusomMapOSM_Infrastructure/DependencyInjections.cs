using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Features.Authentication;
using CusomMapOSM_Infrastructure.Features.Membership;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Infrastructure.Features.User;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Services.Payment;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Shared.Constant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis;
using System.Net.Sockets;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.AccessToolRepo;

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
        services.AddDbContext<CustomMapOSMDbContext>(opt =>
        {
            opt.UseMySql(MySqlDatabase.CONNECTION_STRING, ServerVersion.AutoDetect(MySqlDatabase.CONNECTION_STRING));
        });

        // Register Repositories
        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccessToolRepository, AccessToolRepository>();
        services.AddScoped<IUserAccessToolRepository, UserAccessToolRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Services
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IUserAccessToolService, UserAccessToolService>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

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
        services.AddScoped<StripePaymentService>();
        services.AddScoped<PaypalPaymentService>();
        services.AddScoped<PayOSPaymentService>();
        services.AddScoped<ITransactionService, TransactionService>();

        // Add HttpClient for PayOS
        services.AddHttpClient<PayOSPaymentService>();

        return services;
    }
}
