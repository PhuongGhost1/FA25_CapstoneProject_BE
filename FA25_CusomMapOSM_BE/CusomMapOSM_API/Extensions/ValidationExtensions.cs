using FluentValidation.AspNetCore;

namespace CusomMapOSM_API.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
