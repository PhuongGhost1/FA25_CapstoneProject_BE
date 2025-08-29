using System.Text.Json;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Middlewares;
using CusomMapOSM_Application;
using CusomMapOSM_Infrastructure;
using CusomMapOSM_Infrastructure.Extensions;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Text.Json.Serialization;
using CusomMapOSM_API;
using CusomMapOSM_API.Constants;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Http.Features;


namespace CusomMapOSM_API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../"));
        var envPath = Path.Combine(solutionRoot, ".env");
        Console.WriteLine($"Loading environment variables from: {envPath}");
        Env.Load(envPath);

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
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

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "ready" });

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        // Add Redis Cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis")
                ?? "localhost:6379";
            options.InstanceName = "CustomMapOSM:";
        });

        // Add Template Cache Services
        builder.Services.AddSingleton<CusomMapOSM_Infrastructure.Services.TemplateCacheManager>();
        builder.Services.AddHostedService<CusomMapOSM_Infrastructure.Services.TemplateCacheHostedService>();
        builder.Services.AddEndpoints();
        builder.Services.AddValidation();

        builder.Services.AddSwaggerServices();

        var app = builder.Build();

        app.UseSwaggerServices();
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();

        app.UseHangfireDashboard();

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks(Routes.Health);

        var api = app.MapGroup(Routes.ApiBase);
        app.MapEndpoints(api);

        app.Run();
    }
}
