using System.Diagnostics;

namespace CusomMapOSM_API.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMiddleware
{
    private const double WarningThresholdMs = 5000; // 5 seconds

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var method = context.Request.Method;
        var scheme = context.Request.Scheme;
        var protocol = context.Request.Protocol;
        var path = context.Request.Path;
        var host = context.Request.Host;

        logger.LogInformation(
            "Begin handling request {0} {1} {2}://{3}{4}",
            protocol, method, scheme, host, path
        );

        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        var statusCode = context.Response.StatusCode;

        var message = string.Format(
            "Finish handling request {0} {1} {2}://{3}{4} {5} {6:F2}ms",
            protocol, method, scheme, host, path, statusCode, elapsedMs
        );

        if (elapsedMs >= WarningThresholdMs)
        {
            logger.LogWarning("Slow request detected: " + message);
        }
        else
        {
            logger.LogInformation(message);
        }
    }
}
