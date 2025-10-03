using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Features.Faqs;
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
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Features.Authentication;
using CusomMapOSM_Infrastructure.Features.Faqs;
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
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Infrastructure.Features.Maps;
using CusomMapOSM_Infrastructure.BackgroundJobs;

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
        services.AddDbContext<CustomMapOSMDbContext>(opt =>
        {
            opt.UseMySql(MySqlDatabase.CONNECTION_STRING,
                ServerVersion.AutoDetect(MySqlDatabase.CONNECTION_STRING));
        });

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
        services.AddScoped<IMapFeatureRepository, MapFeatureRepository>();
        services.AddScoped<IFaqRepository, FaqRepository>();

        services
            .AddScoped<CusomMapOSM_Application.Interfaces.Services.Cache.ICacheService,
                CusomMapOSM_Infrastructure.Services.RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IUserAccessToolService, UserAccessToolService>();
        services.AddScoped<IFaqService, FaqService>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<FailedEmailStorageService>();
        services.AddScoped<HangfireEmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IExportQuotaService, ExportQuotaService>();

        // User Repository
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMapFeatureService, MapFeatureService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IGeoJsonService, GeoJsonService>();

        services.AddScoped<IFileProcessorService, Services.FileProcessors.FileProcessorService>();
        services.AddScoped<IVectorProcessor, Services.FileProcessors.VectorProcessor>();
        services.AddScoped<IRasterProcessor, Services.FileProcessors.RasterProcessor>();
        services.AddScoped<ISpreadsheetProcessor, Services.FileProcessors.SpreadsheetProcessor>();

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

        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
        var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        var redisConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";

        services.AddHangfire(config =>
        {
            config.UseRedisStorage(redisConnectionString, new Hangfire.Redis.RedisStorageOptions
            {
                Db = 1
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "email", "fallback" };
        });

        services.AddSingleton<CollaborativeMapService>();

        return services;
    }

    // API web defaults moved to API layer (CusomMapOSM_API.Extensions.WebHostExtensions)

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<FailedEmailStorageService>();
        services.AddScoped<HangfireEmailService>();

        // Register background job services
        services.AddScoped<MembershipExpirationNotificationJob>();
        services.AddScoped<MembershipQuotaResetJob>();
        services.AddScoped<MembershipUsageTrackingJob>();
        services.AddScoped<OrganizationInvitationCleanupJob>();
        services.AddScoped<PaymentFailureHandlingJob>();
        services.AddScoped<ExportFileCleanupJob>();
        services.AddScoped<MapHistoryCleanupJob>();
        services.AddScoped<UserAccountDeactivationJob>();
        services.AddScoped<CollaborationInvitationCleanupJob>();
        // services.AddScoped<SystemLogCleanupJob>();
        services.AddScoped<BackgroundJobScheduler>();

        return services;
    }

    public static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IPaymentService, PaypalPaymentService>();
        services.AddScoped<IPaymentService, PayOSPaymentService>();
        services.AddScoped<IPaymentService, VNPayPaymentService>();

        services.AddHttpClient<PayOSPaymentService>();
        services.AddHttpClient<VNPayPaymentService>();


        return services;
    }
}