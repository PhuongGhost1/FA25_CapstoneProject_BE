using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Features.Authentication;
using CusomMapOSM_Infrastructure.Features.Maps;
using CusomMapOSM_Infrastructure.Features.Membership;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Infrastructure.Features.User;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Services.Payment;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Shared.Constant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis;
using System.Net.Sockets;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.AccessToolRepo;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessTool;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Features.Organization;
using Hangfire;

namespace CusomMapOSM_Infrastructure;

public static class DependencyInjections
{
    private const int RETRY_ATTEMPTS = 3;
    private const int RETRY_DELAY = 10;

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
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
            opt.UseMySql(MySqlDatabase.CONNECTION_STRING,
                ServerVersion.AutoDetect(MySqlDatabase.CONNECTION_STRING));
        });

        // Register Repositories
        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccessToolRepository, AccessToolRepository>();
        services.AddScoped<IUserAccessToolRepository, UserAccessToolRepository>();
        services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IMapRepository, MapRepository>();

        // Cache Services
        services.AddScoped<CusomMapOSM_Application.Interfaces.Services.Cache.ICacheService, CusomMapOSM_Infrastructure.Services.RedisCacheService>();

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
        services.AddScoped<IRabbitMQService, RabbitMqPublisherService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<FailedEmailStorageService>();
        services.AddScoped<HangfireEmailService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IGeoJsonService, GeoJsonService>();
        
        services.AddScoped<IFileProcessorService, Services.FileProcessors.FileProcessorService>();
        services.AddScoped<IVectorProcessor, Services.FileProcessors.VectorProcessor>();
        services.AddScoped<IRasterProcessor, Services.FileProcessors.RasterProcessor>();
        services.AddScoped<ISpreadsheetProcessor, Services.FileProcessors.SpreadsheetProcessor>();

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

        // Configure Hangfire
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
        var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        var redisConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";
        
        services.AddHangfire(config =>
        {
            config.UseRedisStorage(redisConnectionString, new Hangfire.Redis.RedisStorageOptions
            {
                Db = 1 // Use different database for Hangfire
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "email", "fallback" };
        });

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký service để xử lý RabbitMQ connection & queue
        services.AddSingleton<RabbitMqService>();

        // Đăng ký service gửi email và retry
        services.AddScoped<FailedEmailStorageService>();
        services.AddScoped<HangfireEmailService>();

        // Đăng ký background service chính
        services.AddHostedService<EmailProcessingService>();

        return services;
    }

    public static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration)
    {
        // Add payment services
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IPaymentService, PaypalPaymentService>();
        services.AddScoped<IPaymentService, PayOSPaymentService>();
        services.AddScoped<IPaymentService, VNPayPaymentService>();

        // Add HttpClient for PayOS and VNPay
        services.AddHttpClient<PayOSPaymentService>();
        services.AddHttpClient<VNPayPaymentService>();


        // Add payment services here when needed
        return services;
    }
}