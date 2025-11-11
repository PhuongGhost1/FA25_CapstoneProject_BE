using CusomMapOSM_Commons.Constant;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace CusomMapOSM_Application;

public static class DependencyInjections
{
    public static Assembly CurrentAssembly = typeof(DependencyInjections).Assembly; //Assembly.GetExecutingAssembly();
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(CurrentAssembly);

        services.AddAuthorization();
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConstant.JWT_SECRET_KEY))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        var isSignalRHub = path.StartsWithSegments("/api/v1/hubs") || path.StartsWithSegments("/hubs");
                        
                        if (isSignalRHub)
                        {
                            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                            var authHeader = context.Request.Headers["Authorization"].ToString();
                            
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            else if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                            }
                        }
                        
                        return Task.CompletedTask;
                    }
                };
                options.UseSecurityTokenValidators = true;
            }
        );

        services.AddHttpContextAccessor();

        services.AddCors(setup =>
        {
            setup.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });
        });

        // services.AddRateLimiter(_ => _
        // .AddFixedWindowLimiter(policyName: "fixed", options =>
        // {
        //     options.PermitLimit = 4;
        //     options.Window = TimeSpan.FromSeconds(12);
        //     options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        //     options.QueueLimit = 2;
        // }));

        // Register Services

        return services;
    }
}
