using System.Text.Json;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Middlewares;
using CusomMapOSM_Application;
using CusomMapOSM_Infrastructure;
using CusomMapOSM_Infrastructure.Extensions;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using CusomMapOSM_API;
using CusomMapOSM_API.Constants;
using CusomMapOSM_Commons.Constant;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Http.Features;
using CusomMapOSM_Infrastructure.Hubs;
using CusomMapOSM_Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Infrastructure.Databases;

namespace CusomMapOSM_API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddFilter(
            "Microsoft.EntityFrameworkCore.Database.Command",
            LogLevel.Warning);

        var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../"));
        var envPath = Path.Combine(solutionRoot, ".env");
        Console.WriteLine($"Loading environment variables from: {envPath}");
        Env.Load(envPath);
        var origins = FrontendConstant.FRONTEND_ORIGINS
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase,
                allowIntegerValues: true));
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase,
                allowIntegerValues: true));
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        builder.Services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
        });

        builder.Services.AddSingleton<ExceptionMiddleware>();
        builder.Services.AddSingleton<LoggingMiddleware>();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddEndpoints();
        builder.Services.AddValidation();
        builder.Services.AddSwaggerServices();

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendCors", policy =>
                (origins.Length > 0
                    ? policy.WithOrigins(origins)
                    : policy.SetIsOriginAllowed(_ => true)
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );
        });

        builder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 102400;
            options.EnableDetailedErrors = true;
            options.StreamBufferCapacity = 20;
        });
        
        var app = builder.Build();
        
      // app.UseInitializeDatabase();
        app.UseSwaggerServices();
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<QuotaCheckingMiddleware>();

        app.UseHangfireDashboard();

        // Register all recurring background jobs
        using (var scope = app.Services.CreateScope())
        {
            var scheduler = scope.ServiceProvider.GetRequiredService<CusomMapOSM_Infrastructure.BackgroundJobs.BackgroundJobScheduler>();
            scheduler.RegisterAllRecurringJobs();
        }

        app.UseCors("FrontendCors");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks(Routes.Health);

        var api = app.MapGroup(Routes.ApiBase);
        app.MapEndpoints(api);

        app.MapHub<StoryHub>("/hubs/story")
        .RequireCors("FrontendCors");
        api.MapHub<NotificationHub>("/hubs/notifications")
            .RequireCors("FrontendCors")
            .RequireAuthorization();
        api.MapHub<MapCollaborationHub>("/hubs/mapCollaboration")
            .RequireCors("FrontendCors")
            .RequireAuthorization();
        api.MapHub<SessionHub>("/hubs/session")
            .RequireCors("FrontendCors");
        api.MapHub<GroupCollaborationHub>("/hubs/groupCollaboration")
            .RequireCors("FrontendCors");
        api.MapHub<SupportTicketHub>("/hubs/support-tickets")
            .RequireCors("FrontendCors");
        app.Run();
    }
}