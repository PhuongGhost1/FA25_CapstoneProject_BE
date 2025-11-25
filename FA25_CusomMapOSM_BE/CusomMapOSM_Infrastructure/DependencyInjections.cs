using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Interfaces.Features.Faqs;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Features.Transaction;
using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Features.Animations;
using CusomMapOSM_Application.Interfaces.Features.Workspace;
using CusomMapOSM_Application.Interfaces.Features.Home;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Interfaces.Features.OrganizationAdmin;
using CusomMapOSM_Application.Interfaces.Features.SupportTicket;
using CusomMapOSM_Application.Interfaces.Features.SystemAdmin;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Interfaces.Services.Payment;
using CusomMapOSM_Application.Interfaces.Services.OSM;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Interfaces.Services.Maps;
using CusomMapOSM_Application.Interfaces.Services.Organization;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using System.Net.Http;
using System.Net.Security;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Notifications;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Workspace;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Notifications;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Animations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspace;
using CusomMapOSM_Infrastructure.Features.Authentication;
using CusomMapOSM_Infrastructure.Features.Faqs;
using CusomMapOSM_Infrastructure.Features.Maps;
using CusomMapOSM_Infrastructure.Features.Membership;
using CusomMapOSM_Infrastructure.Features.Transaction;
using CusomMapOSM_Infrastructure.Features.User;
using CusomMapOSM_Infrastructure.Features.Usage;
using CusomMapOSM_Infrastructure.Features.Payment;
using CusomMapOSM_Infrastructure.Features.StoryMaps;
using CusomMapOSM_Infrastructure.Features.Animations;
using CusomMapOSM_Infrastructure.Features.Workspace;
using CusomMapOSM_Infrastructure.Features.Home;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Services.Payment;
using CusomMapOSM_Infrastructure.Services.Maps.Mongo;
using CusomMapOSM_Infrastructure.Services.StoryMaps;
using CusomMapOSM_Infrastructure.Services.LayerData.Mongo;
using CusomMapOSM_Infrastructure.Services.MapFeatures.Mongo;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.OrganizationAdmin;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.SystemAdmin;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SystemAdmin;
using CusomMapOSM_Infrastructure.Features.OrganizationAdmin;
using CusomMapOSM_Infrastructure.Features.SupportTicket;
using CusomMapOSM_Infrastructure.Features.SystemAdmin;
using CusomMapOSM_Infrastructure.Services.FileProcessors;
using CusomMapOSM_Infrastructure.Services.LayerData.Relational;
using CusomMapOSM_Infrastructure.Services.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Features.Organization;
using CusomMapOSM_Infrastructure.BackgroundJobs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SupportTicket;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.SupportTicket;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis;
using System.Net.Sockets;
using CusomMapOSM_Application.Interfaces.Features.Groups;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Interfaces.Features.QuestionBanks;
using CusomMapOSM_Application.Interfaces.Features.QuickPolls;
using CusomMapOSM_Application.Interfaces.Features.Sessions;
using CusomMapOSM_Application.Interfaces.Features.TreasureHunts;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using CusomMapOSM_Infrastructure.Features.Groups;
using CusomMapOSM_Infrastructure.Features.Locations;
using CusomMapOSM_Infrastructure.Features.Notifications;
using CusomMapOSM_Infrastructure.Features.QuestionBanks;
using CusomMapOSM_Infrastructure.Features.QuickPolls;
using CusomMapOSM_Infrastructure.Features.Sessions;
using CusomMapOSM_Infrastructure.Features.TreasureHunts;
using Hangfire;
using Hangfire.Redis;

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
                Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(MySqlDatabase.CONNECTION_STRING));
        });

        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
        services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuestionOptionRepository, QuestionOptionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionParticipantRepository, SessionParticipantRepository>();
        services.AddScoped<ISessionQuestionRepository, SessionQuestionRepository>();
        services.AddScoped<IStudentResponseRepository, StudentResponseRepository>();
        services.AddScoped<ISessionGroupRepository, SessionGroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<IGroupSubmissionRepository, GroupSubmissionRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IMapRepository, MapRepository>();
        services.AddScoped<IMapQuestionBankRepository, MapQuestionBankRepository>();
        services.AddScoped<IMapFeatureRepository, MapFeatureRepository>();
        services.AddScoped<IMapHistoryRepository, MapHistoryRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IFaqRepository, FaqRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IStoryMapRepository, StoryMapRepository>();
        services.AddScoped<ILayerAnimationRepository, LayerAnimationRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        
        services.AddSingleton<IMongoClient>(_ => new MongoClient(MongoDatabaseConstant.ConnectionString));
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(MongoDatabaseConstant.DatabaseName);
        });
        
        services.AddScoped<RelationalLayerDataStore>();
        services.AddScoped<MongoLayerDataStore>();
        services.AddScoped<ILayerDataStore>(sp => sp.GetRequiredService<MongoLayerDataStore>());
        
        services.AddScoped<IMapFeatureStore, MongoMapFeatureStore>();
        services.AddScoped<IMapHistoryStore, MongoMapHistoryStore>();
        

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUsageService, UsageService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<HtmlContentImageProcessor>();
        services.AddScoped<IStoryMapService, StoryMapService>();
        services.AddScoped<IMapSelectionService, MapSelectionService>();
        services.AddScoped<ISegmentExecutor, SegmentExecutor>();
        services.AddSingleton<ISegmentExecutionStateStore, InMemorySegmentExecutionStateStore>();
        services.AddScoped<ILayerAnimationService, LayerAnimationService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IQuickPollService, QuickPollService>();
        services.AddScoped<ITreasureHuntService, TreasureHuntService>();
        services.AddScoped<IGroupCollaborationService, GroupCollaborationService>();

        services.AddScoped<IOrganizationAdminService, OrganizationAdminService>();
        services.AddScoped<IOrganizationAdminRepository, OrganizationAdminRepository>();

        // System Admin Services
        services.AddScoped<ISystemAdminService, SystemAdminService>();
        services.AddScoped<ISystemAdminRepository, SystemAdminRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<HangfireEmailService>();
        // Register email notification service (from Services namespace)
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<IExportQuotaService, ExportQuotaService>();
        services.AddHttpClient<IOsmService, OsmService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Allow OSRM public API even if certificate validation fails
                if (message.RequestUri?.Host.Contains("project-osrm.org") == true)
                {
                    return true;
                }
                return errors == System.Net.Security.SslPolicyErrors.None;
            }
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services.AddScoped<IQuestionBankService, QuestionBankService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMapFeatureService, MapFeatureService>();
        services.AddScoped<IMapHistoryService, MapHistoryService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IGeoJsonService, GeoJsonService>();
        services.AddSingleton<IStoryBroadcastService, StoryBroadcastService>();
        services.AddScoped<IOrganizationPermissionService, OrganizationPermissionService>();
        
        // Home service for aggregated statistics
        services.AddScoped<IHomeService, HomeService>();

        services.AddScoped<IFileProcessorService, FileProcessorService>();
        services.AddScoped<IVectorProcessor, VectorProcessor>();
        services.AddScoped<IRasterProcessor, RasterProcessor>();
        services.AddScoped<ISpreadsheetProcessor, SpreadsheetProcessor>();
        
        // Firebase Storage service
        services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = RedisConstant.REDIS_CONNECTION_STRING;
            options.InstanceName = "IMOS:";
        });
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {


            var redisConnectionString = RedisConstant.REDIS_CONNECTION_STRING;

            var policy = Policy
                .Handle<RedisConnectionException>()
                .Or<SocketException>()
                .WaitAndRetry(RETRY_ATTEMPTS, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            return policy.Execute(() => ConnectionMultiplexer.Connect(redisConnectionString));
        });
        

        services.AddHangfire(config =>
        {
            config.UseRedisStorage(RedisConstant.REDIS_CONNECTION_STRING, new RedisStorageOptions
            {
                Db = 1
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "email" };
        });

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddScoped<MapSelectionCleanupJob>();
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
