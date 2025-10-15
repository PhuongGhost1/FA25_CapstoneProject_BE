using CusomMapOSM_Commons.Configuration;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Services;

public interface IExportQuotaService
{
    Task<bool> CanExportAsync(Guid userId, string fileType, int fileSizeKB);
    Task<int> CalculateTokenCostAsync(string fileType, int fileSizeKB);
    Task<bool> ConsumeTokensAsync(Guid userId, int tokens);
    Task<int> GetRemainingTokensAsync(Guid userId);
    Task<int> GetTotalTokensAsync(Guid userId);
    Task ResetMonthlyTokensAsync(Guid userId);
    Task<bool> IsFileSizeAllowedAsync(int fileSizeKB);
}

public class ExportQuotaService : IExportQuotaService
{
    private readonly ILogger<ExportQuotaService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ExportQuotaSettings _quotaSettings;

    public ExportQuotaService(
        ILogger<ExportQuotaService> logger,
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _quotaSettings = StatusConfiguration.GetConfig().ExportQuotaSettings;
    }

    public async Task<bool> CanExportAsync(Guid userId, string fileType, int fileSizeKB)
    {
        try
        {
            // Check file size limit
            if (!await IsFileSizeAllowedAsync(fileSizeKB))
            {
                _logger.LogWarning("File size {FileSizeKB}KB exceeds maximum allowed size for user {UserId}", fileSizeKB, userId);
                return false;
            }

            // Calculate token cost
            var tokenCost = await CalculateTokenCostAsync(fileType, fileSizeKB);

            // Check if user has enough tokens
            var remainingTokens = await GetRemainingTokensAsync(userId);

            if (remainingTokens < tokenCost)
            {
                _logger.LogWarning("Insufficient tokens for user {UserId}. Required: {TokenCost}, Available: {RemainingTokens}",
                    userId, tokenCost, remainingTokens);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking export quota for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> CalculateTokenCostAsync(string fileType, int fileSizeKB)
    {
        try
        {
            // Get base cost for file type
            var baseCost = _quotaSettings.TokenCosts.GetValueOrDefault(fileType.ToUpper(), 1);

            // Calculate size-based cost (1KB = 100 tokens as base)
            var sizeCost = (int)Math.Ceiling(fileSizeKB * (_quotaSettings.TokenPerKB / 100.0));

            // Total cost = base cost + size cost
            var totalCost = baseCost + sizeCost;

            _logger.LogDebug("Calculated token cost for {FileType} ({FileSizeKB}KB): {TotalCost} tokens",
                fileType, fileSizeKB, totalCost);

            return totalCost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating token cost for {FileType} ({FileSizeKB}KB)", fileType, fileSizeKB);
            return 1; // Default minimum cost
        }
    }

    public async Task<bool> ConsumeTokensAsync(Guid userId, int tokens)
    {
        try
        {
            var remainingTokens = await GetRemainingTokensAsync(userId);
            if (remainingTokens < tokens)
            {
                _logger.LogWarning("Cannot consume {Tokens} tokens for user {UserId}. Only {RemainingTokens} available",
                    tokens, userId, remainingTokens);
                return false;
            }

            // Get current user token usage
            var currentUsage = await _userRepository.GetUserTokenUsageAsync(userId);
            var newUsage = currentUsage + tokens;

            // Update user's token balance in database
            var success = await _userRepository.UpdateUserTokenUsageAsync(userId, newUsage);
            if (!success)
            {
                _logger.LogError("Failed to update token usage for user {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Consumed {Tokens} tokens for user {UserId}. New usage: {NewUsage}",
                tokens, userId, newUsage);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming tokens for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetRemainingTokensAsync(Guid userId)
    {
        try
        {
            var totalTokens = await GetTotalTokensAsync(userId);
            var usedTokens = await GetUsedTokensAsync(userId);

            return Math.Max(0, totalTokens - usedTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining tokens for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<int> GetTotalTokensAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetUserTotalTokensAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total tokens for user {UserId}", userId);
            return 0;
        }
    }

    public async Task ResetMonthlyTokensAsync(Guid userId)
    {
        try
        {
            var success = await _userRepository.ResetUserMonthlyTokensAsync(userId);
            if (success)
            {
                _logger.LogInformation("Reset monthly tokens for user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to reset monthly tokens for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting monthly tokens for user {UserId}", userId);
        }
    }

    public async Task<bool> IsFileSizeAllowedAsync(int fileSizeKB)
    {
        try
        {
            var maxSizeKB = _quotaSettings.MaxFileSizeMB * 1024;
            return fileSizeKB <= maxSizeKB;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file size limit for {FileSizeKB}KB", fileSizeKB);
            return false;
        }
    }

    private async Task<int> GetUsedTokensAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetUserTokenUsageAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting used tokens for user {UserId}", userId);
            return 0;
        }
    }
}

// Extension methods for easier token cost calculation
public static class ExportQuotaExtensions
{
    public static string GetFileTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToUpper().TrimStart('.');
        return extension switch
        {
            "PNG" => "PNG",
            "JPG" or "JPEG" => "JPG",
            "PDF" => "PDF",
            "GEOJSON" or "JSON" => "GeoJSON",
            "KML" => "KML",
            "SHP" or "SHAPEFILE" => "Shapefile",
            "MBTILES" => "MBTiles",
            _ => "PNG" // Default fallback
        };
    }

    public static int GetFileSizeKB(byte[] fileData)
    {
        return (int)Math.Ceiling(fileData.Length / 1024.0);
    }

    public static string FormatFileSize(int sizeKB)
    {
        if (sizeKB < 1024)
            return $"{sizeKB} KB";
        else if (sizeKB < 1024 * 1024)
            return $"{sizeKB / 1024.0:F1} MB";
        else
            return $"{sizeKB / (1024.0 * 1024.0):F1} GB";
    }
}
