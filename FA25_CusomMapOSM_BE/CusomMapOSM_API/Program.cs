using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Middlewares;
using CusomMapOSM_Application;
using CusomMapOSM_Infrastructure;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
            );
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        // Configure JSON serialization for controllers
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
            );
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        // Add middleware to the container.
        builder.Services.AddSingleton<ExceptionMiddleware>();
        builder.Services.AddSingleton<LoggingMiddleware>();

        // Add health checks to the container.
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "ready" });

        // Add services to the container.
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddEndpoints();
        builder.Services.AddValidation();

        // Add swagger services to the container.
        builder.Services.AddSwaggerServices();

        var app = builder.Build();

        app.UseSwaggerServices();
        app.UseHttpsRedirection();

        // Use custom middlewares
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");

        // Map all endpoints
        app.MapEndpoints();

        app.Run();
    }
}
