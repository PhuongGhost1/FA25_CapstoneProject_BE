using System.Text.Json;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapSelectionService : IMapSelectionService
{
    private readonly IDistributedCache _redis;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MapSelectionService> _logger;
    private static readonly string[] HighlightColors = new[]
    {
        "#FF5733", "#33FF57", "#3357FF", "#F333FF", "#FF33F3",
        "#33FFF3", "#F3FF33", "#FF8C33", "#8C33FF", "#33FF8C"
    };
    private const int SelectionTtlSeconds = 300; // 5 minutes
    private const int UserInfoTtlSeconds = 3600; // 1 hour
    public MapSelectionService(
        IDistributedCache redis,
        ICurrentUserService currentUserService,
        ILogger<MapSelectionService> logger)
    {
        _redis = redis;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Option<bool, Error>> UserJoinMap(Guid mapId, Guid userId, string connectionId)
    {
        try
        {
            var userInfo = await GetOrCreateUserInfo(userId);
            var activeUsersKey = GetActiveUsersKey(mapId);
            var activeUsers = await GetSetFromRedis<Guid>(activeUsersKey) ?? new HashSet<Guid>();
            activeUsers.Add(userId);
            await SaveSetToRedis(activeUsersKey, activeUsers, TimeSpan.FromSeconds(SelectionTtlSeconds));
            var connectionMapKey = GetConnectionMapKey(connectionId);
            await _redis.SetStringAsync(connectionMapKey, mapId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SelectionTtlSeconds)
            });

            _logger.LogInformation("User {UserId} joined map {MapId} with connection {ConnectionId}", userId, mapId, connectionId);

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining map {MapId} for user {UserId}", mapId, userId);
            return Option.None<bool, Error>(Error.New($"Failed to join map: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> UserLeaveMap(Guid mapId, Guid userId)
    {
        try
        {
            var activeUsersKey = GetActiveUsersKey(mapId);
            var activeUsers = await GetSetFromRedis<Guid>(activeUsersKey);
            if (activeUsers != null)
            {
                activeUsers.Remove(userId);
                if (activeUsers.Any())
                {
                    await SaveSetToRedis(activeUsersKey, activeUsers, TimeSpan.FromSeconds(SelectionTtlSeconds));
                }
                else
                {
                    await _redis.RemoveAsync(activeUsersKey);
                }
            }
            var selectionKey = GetSelectionKey(mapId, userId);
            await _redis.RemoveAsync(selectionKey);
            _logger.LogInformation("User {UserId} left map {MapId}", userId, mapId);
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving map {MapId} for user {UserId}", mapId, userId);
            return Option.None<bool, Error>(Error.New($"Failed to leave map: {ex.Message}"));
        }
    }

    public async Task<Option<MapSelectionResponse, Error>> UpdateSelection(
        Guid mapId,
        Guid userId,
        UpdateSelectionRequest request)
    {
        try
        {
            var userInfo = await GetOrCreateUserInfo(userId);
            var selection = new MapSelectionResponse
            {
                UserId = userId,
                UserName = userInfo.UserName,
                UserAvatar = userInfo.UserAvatar,
                SelectionType = request.SelectionType,
                SelectedObjectId = request.SelectedObjectId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                SelectedAt = DateTime.UtcNow,
                HighlightColor = userInfo.HighlightColor
            };
            var key = GetSelectionKey(mapId, userId);
            var json = JsonSerializer.Serialize(selection);
            await _redis.SetStringAsync(key, json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SelectionTtlSeconds)
                });
            await RefreshActiveUser(mapId, userId);
            _logger.LogInformation("User {UserId} selected {SelectionType}:{ObjectId} on map {MapId}", userId, request.SelectionType, request.SelectedObjectId, mapId);
            return Option.Some<MapSelectionResponse, Error>(selection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating selection for user {UserId} on map {MapId}", userId, mapId);
            return Option.None<MapSelectionResponse, Error>(Error.New($"Failed to update selection: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ClearSelection(Guid mapId, Guid userId)
    {
        try
        {
            var key = GetSelectionKey(mapId, userId);
            await _redis.RemoveAsync(key);
            _logger.LogInformation("User {UserId} cleared selection on map {MapId}", userId, mapId);
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing selection for user {UserId} on map {MapId}", userId, mapId);
            return Option.None<bool, Error>(Error.New($"Failed to clear selection: {ex.Message}"));
        }   
    }

    public async Task<Option<MapSelectionResponse, Error>> GetUserSelection(Guid mapId, Guid userId)
    {
        try
        {
            var key = GetSelectionKey(mapId, userId);
            var json = await _redis.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return Option.None<MapSelectionResponse, Error>(Error.New("No selection found"));
            }
            var selection = JsonSerializer.Deserialize<MapSelectionResponse>(json);
            return Option.Some<MapSelectionResponse, Error>(selection!);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting selection for user {UserId} on map {MapId}", userId, mapId);
            return Option.None<MapSelectionResponse, Error>(Error.New($"Failed to get selection: {ex.Message}"));
        }
    }

    public async Task<Option<List<ActiveMapUserResponse>, Error>> GetActiveUsers(Guid mapId)
    {

        try
        {
            var activeUsersKey = GetActiveUsersKey(mapId);
            var activeUserIds = await GetSetFromRedis<Guid>(activeUsersKey);
            if (activeUserIds == null || !activeUserIds.Any())
            {
                return Option.Some<List<ActiveMapUserResponse>, Error>(new List<ActiveMapUserResponse>());
            }
            var activeUsers = new List<ActiveMapUserResponse>();
            foreach (var userId in activeUserIds)
            {
                var userInfo = await GetOrCreateUserInfo(userId);
                var selectionResult = await GetUserSelection(mapId, userId);
                var activeUser = new ActiveMapUserResponse
                {
                    UserId = userId,
                    UserName = userInfo.UserName,
                    UserAvatar = userInfo.UserAvatar,
                    HighlightColor = userInfo.HighlightColor,
                    JoinedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,
                    IsIdle = false,

                    CurrentSelection = selectionResult.Match(

                        selection => selection,
                        error => null
                    )
                };
                activeUsers.Add(activeUser);
            }
            return Option.Some<List<ActiveMapUserResponse>, Error>(activeUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users for map {MapId}", mapId);
            return Option.None<List<ActiveMapUserResponse>, Error>(Error.New($"Failed to get active users: {ex.Message}"));
        }
    }

    public async Task CleanupInactiveSelections(Guid mapId)
    {
        try
        {
            var activeUsersKey = GetActiveUsersKey(mapId);
            var activeUserIds = await GetSetFromRedis<Guid>(activeUsersKey);
            if (activeUserIds == null || !activeUserIds.Any())
            {
                _logger.LogInformation("No active users to cleanup for map {MapId}", mapId);
                return;
            }
            var removedCount = 0;
            foreach (var userId in activeUserIds.ToList())
            {
                var selectionKey = GetSelectionKey(mapId, userId);
                var exists = await _redis.GetStringAsync(selectionKey);
                if (string.IsNullOrEmpty(exists))
                {
                    activeUserIds.Remove(userId);
                    removedCount++;
                }
            }
            if (removedCount > 0)
            {
                if (activeUserIds.Any())
                {
                    await SaveSetToRedis(activeUsersKey, activeUserIds, TimeSpan.FromSeconds(SelectionTtlSeconds));
                }
                else
                {
                    await _redis.RemoveAsync(activeUsersKey);
                }
                _logger.LogInformation(
                    "Cleaned up {Count} inactive users from map {MapId}",
                    removedCount, mapId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up selections for map {MapId}", mapId);
        }

    }

 

    #region Private Helpers

    private string GetSelectionKey(Guid mapId, Guid userId)
        => $"map:{mapId}:selection:{userId}";

    private string GetActiveUsersKey(Guid mapId)
        => $"map:{mapId}:active-users";

    private string GetUserInfoKey(Guid userId)
        => $"user:{userId}:info";

    private string GetConnectionMapKey(string connectionId)
        => $"connection:{connectionId}:map";

    public async Task<Option<Guid, Error>> GetMapIdFromConnection(string connectionId)
    {
        try
        {
            var connectionMapKey = GetConnectionMapKey(connectionId);
            var mapIdString = await _redis.GetStringAsync(connectionMapKey);
            
            if (string.IsNullOrEmpty(mapIdString))
            {
                return Option.None<Guid, Error>(Error.New("Connection not found"));
            }
            
            if (Guid.TryParse(mapIdString, out var mapId))
            {
                return Option.Some<Guid, Error>(mapId);
            }
            
            return Option.None<Guid, Error>(Error.New("Invalid map ID format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mapId from connection {ConnectionId}", connectionId);
            return Option.None<Guid, Error>(Error.New($"Failed to get mapId: {ex.Message}"));
        }
    }
    public async Task RemoveConnectionMapping(string connectionId)
    {
        try
        {
            var connectionMapKey = GetConnectionMapKey(connectionId);
            await _redis.RemoveAsync(connectionMapKey);
            _logger.LogInformation("Removed connection mapping for {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection mapping for {ConnectionId}", connectionId);
        }
    }
    public async Task<Option<string, Error>> GetUserHighlightColor(Guid userId)
    {
        try
        {
            var userInfo = await GetOrCreateUserInfo(userId);
            return Option.Some<string, Error>(userInfo.HighlightColor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user highlight color for {UserId}", userId);
            return Option.None<string, Error>(Error.New($"Failed to get user highlight color: {ex.Message}"));
        }
    }

    private async Task<UserInfoCache> GetOrCreateUserInfo(Guid userId)
    {
        var key = GetUserInfoKey(userId);
        var json = await _redis.GetStringAsync(key);
        if (!string.IsNullOrEmpty(json))
        {
            return JsonSerializer.Deserialize<UserInfoCache>(json)!;
        }
        var userInfo = new UserInfoCache
        {
            UserId = userId,
            UserName = _currentUserService.GetEmail() ?? "Unknown User",
            UserAvatar = "", // Get from user service if available
            HighlightColor = AssignColorToUser(userId)
        };
        await _redis.SetStringAsync(
            key,
            JsonSerializer.Serialize(userInfo),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(UserInfoTtlSeconds)
            });
        return userInfo;
    }

    private async Task RefreshActiveUser(Guid mapId, Guid userId)
    {
        var key = GetActiveUsersKey(mapId);
        var activeUsers = await GetSetFromRedis<Guid>(key) ?? new HashSet<Guid>();
        activeUsers.Add(userId);
        await SaveSetToRedis(key, activeUsers, TimeSpan.FromSeconds(SelectionTtlSeconds));
    }
    
    private async Task<HashSet<T>?> GetSetFromRedis<T>(string key)
    {
        var json = await _redis.GetStringAsync(key);
        if (string.IsNullOrEmpty(json))
            return null;
        return JsonSerializer.Deserialize<HashSet<T>>(json);
    }

    private async Task SaveSetToRedis<T>(string key, HashSet<T> set, TimeSpan expiration)
    {
        var json = JsonSerializer.Serialize(set);
        await _redis.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {

            AbsoluteExpirationRelativeToNow = expiration
        });

    }

    private string AssignColorToUser(Guid userId)
    {
        var hash = userId.GetHashCode();
        var index = Math.Abs(hash) % HighlightColors.Length;
        return HighlightColors[index];
    }
    #endregion
}


internal class UserInfoCache
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string HighlightColor { get; set; } = string.Empty;
}