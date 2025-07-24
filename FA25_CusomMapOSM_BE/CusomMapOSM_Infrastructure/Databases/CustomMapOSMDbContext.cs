using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases;

public class CustomMapOSMDbContext : DbContext
{
    public CustomMapOSMDbContext(DbContextOptions<CustomMapOSMDbContext> options) : base(options) { }

    public CustomMapOSMDbContext()
    {    
    }

    // DbSet properties for your entities here
    #region DbSet Properties

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomMapOSMDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
