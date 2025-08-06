using CusomMapOSM_Shared.Constant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace CusomMapOSM_Infrastructure.Databases;

public class LmsDbContextFactory : IDesignTimeDbContextFactory<CustomMapOSMDbContext>
{
    public CustomMapOSMDbContext CreateDbContext(string[] args)
    {
        string envPath = Path.GetFullPath(Path.Combine
            (AppDomain.CurrentDomain.BaseDirectory, "../../../../../FA25_CusomMapOSM_BE/.env"));
        Console.WriteLine("envPath in Infrastructure: " + envPath);
        Env.Load(envPath);

        var optionsBuilder = new DbContextOptionsBuilder<CustomMapOSMDbContext>();

        var connectionString = MySqlDatabase.CONNECTION_STRING;

        optionsBuilder.UseMySql(connectionString, ServerVersion.Parse("8.0.34-mysql"));

        return new CustomMapOSMDbContext(optionsBuilder.Options);
    }
}