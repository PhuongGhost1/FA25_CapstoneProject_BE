using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.Databases;

public static class DbInitializer
{
    public static IApplicationBuilder UseInitializeDatabase(this IApplicationBuilder application)
    {
        using var serviceScope = application.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetService<CustomMapOSMDbContext>();

        if (dbContext != null && dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying  Migrations...");
            dbContext.Database.Migrate();
        }
        throw new Exception("Pending Migrations");
    }
}