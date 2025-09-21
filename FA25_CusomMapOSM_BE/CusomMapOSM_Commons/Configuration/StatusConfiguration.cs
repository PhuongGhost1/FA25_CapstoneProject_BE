using System.Text.Json;

namespace CusomMapOSM_Commons.Configuration;

public class StatusConfiguration
{
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "status-config.json");
    private static StatusConfig? _config;
    private static readonly object _lock = new object();

    public static StatusConfig GetConfig()
    {
        if (_config == null)
        {
            lock (_lock)
            {
                if (_config == null)
                {
                    LoadConfiguration();
                }
            }
        }
        return _config!;
    }

    private static void LoadConfiguration()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _config = JsonSerializer.Deserialize<StatusConfig>(json);
            }
            else
            {
                // Create default configuration
                _config = CreateDefaultConfiguration();
                SaveConfiguration(_config);
            }
        }
        catch (Exception ex)
        {
            // Fallback to default configuration
            _config = CreateDefaultConfiguration();
        }
    }

    public static void SaveConfiguration(StatusConfig config)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            Console.WriteLine($"Failed to save status configuration: {ex.Message}");
        }
    }

    private static StatusConfig CreateDefaultConfiguration()
    {
        return new StatusConfig
        {
            AccountStatuses = new List<StatusItem>
            {
                new() { Id = 1, Name = "Active", Description = "Account is active and can use the system" },
                new() { Id = 2, Name = "Inactive", Description = "Account is inactive" },
                new() { Id = 3, Name = "Suspended", Description = "Account is suspended" },
                new() { Id = 4, Name = "PendingVerification", Description = "Account is pending email verification" }
            },
            MembershipStatuses = new List<StatusItem>
            {
                new() { Id = 1, Name = "Active", Description = "Membership is active" },
                new() { Id = 2, Name = "Expired", Description = "Membership has expired" },
                new() { Id = 3, Name = "Suspended", Description = "Membership is suspended" },
                new() { Id = 4, Name = "PendingPayment", Description = "Membership is pending payment" },
                new() { Id = 5, Name = "Cancelled", Description = "Membership is cancelled" }
            },
            TicketStatuses = new List<StatusItem>
            {
                new() { Id = 1, Name = "Open", Description = "Ticket is open" },
                new() { Id = 2, Name = "InProgress", Description = "Ticket is in progress" },
                new() { Id = 3, Name = "WaitingForCustomer", Description = "Waiting for customer response" },
                new() { Id = 4, Name = "Resolved", Description = "Ticket is resolved" },
                new() { Id = 5, Name = "Closed", Description = "Ticket is closed" }
            },
            ExportQuotaSettings = new ExportQuotaSettings
            {
                TokenPerKB = 100,
                MaxFileSizeMB = 500,
                TokenCosts = new Dictionary<string, int>
                {
                    { "PNG", 1 },
                    { "JPG", 1 },
                    { "PDF", 2 },
                    { "GeoJSON", 3 },
                    { "KML", 3 },
                    { "Shapefile", 5 },
                    { "MBTiles", 10 }
                }
            }
        };
    }
}

public class StatusConfig
{
    public List<StatusItem> AccountStatuses { get; set; } = new();
    public List<StatusItem> MembershipStatuses { get; set; } = new();
    public List<StatusItem> TicketStatuses { get; set; } = new();
    public ExportQuotaSettings ExportQuotaSettings { get; set; } = new();
}

public class StatusItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ExportQuotaSettings
{
    public int TokenPerKB { get; set; } = 100;
    public int MaxFileSizeMB { get; set; } = 500;
    public Dictionary<string, int> TokenCosts { get; set; } = new();
}
