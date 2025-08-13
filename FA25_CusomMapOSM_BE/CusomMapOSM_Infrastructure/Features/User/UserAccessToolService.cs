using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.User;

public class UserAccessToolService : IUserAccessToolService
{
    private readonly IUserAccessToolRepository _userAccessToolRepository;
    private readonly IAccessToolRepository _accessToolRepository;
    private readonly IMembershipPlanRepository _membershipPlanRepository;

    public UserAccessToolService(
        IUserAccessToolRepository userAccessToolRepository,
        IAccessToolRepository accessToolRepository,
        IMembershipPlanRepository membershipPlanRepository)
    {
        _userAccessToolRepository = userAccessToolRepository;
        _accessToolRepository = accessToolRepository;
        _membershipPlanRepository = membershipPlanRepository;
    }

    public async Task<Option<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>> GetUserAccessToolsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var userAccessTools = await _userAccessToolRepository.GetByUserIdAsync(userId, ct);
            return Option.Some<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>(userAccessTools);
        }
        catch (Exception ex)
        {
            return Option.None<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.GetFailed", $"Failed to get user access tools: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>> GetActiveUserAccessToolsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var userAccessTools = await _userAccessToolRepository.GetActiveByUserIdAsync(userId, ct);
            return Option.Some<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>(userAccessTools);
        }
        catch (Exception ex)
        {
            return Option.None<IReadOnlyList<UserAccessTool>, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.GetActiveFailed", $"Failed to get active user access tools: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<bool, ErrorCustom.Error>> HasAccessToToolAsync(Guid userId, int accessToolId, CancellationToken ct)
    {
        try
        {
            var hasAccess = await _userAccessToolRepository.HasAccessAsync(userId, accessToolId, ct);
            return Option.Some<bool, ErrorCustom.Error>(hasAccess);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.CheckAccessFailed", $"Failed to check access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<UserAccessTool, ErrorCustom.Error>> GrantAccessToToolAsync(Guid userId, int accessToolId, DateTime expiredAt, CancellationToken ct)
    {
        try
        {
            // Check if access tool exists
            var accessTool = await _accessToolRepository.GetByIdAsync(accessToolId, ct);
            if (accessTool == null)
            {
                return Option.None<UserAccessTool, ErrorCustom.Error>(
                    new ErrorCustom.Error("UserAccessTool.AccessToolNotFound", "Access tool not found", ErrorCustom.ErrorType.NotFound));
            }

            // Check if user already has access to this tool
            var existingAccess = await _userAccessToolRepository.GetByUserAndToolAsync(userId, accessToolId, ct);
            if (existingAccess != null)
            {
                // Update existing access
                existingAccess.ExpiredAt = expiredAt;
                var updatedAccess = await _userAccessToolRepository.UpdateAsync(existingAccess, ct);
                return Option.Some<UserAccessTool, ErrorCustom.Error>(updatedAccess);
            }

            // Create new access
            var userAccessTool = new UserAccessTool
            {
                UserId = userId,
                AccessToolId = accessToolId,
                GrantedAt = DateTime.UtcNow,
                ExpiredAt = expiredAt
            };

            var createdAccess = await _userAccessToolRepository.CreateAsync(userAccessTool, ct);
            return Option.Some<UserAccessTool, ErrorCustom.Error>(createdAccess);
        }
        catch (Exception ex)
        {
            return Option.None<UserAccessTool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.GrantAccessFailed", $"Failed to grant access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<bool, ErrorCustom.Error>> RevokeAccessToToolAsync(Guid userId, int accessToolId, CancellationToken ct)
    {
        try
        {
            var revoked = await _userAccessToolRepository.DeleteByUserAndToolAsync(userId, accessToolId, ct);
            return Option.Some<bool, ErrorCustom.Error>(revoked);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.RevokeAccessFailed", $"Failed to revoke access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<bool, ErrorCustom.Error>> GrantAccessToToolsAsync(Guid userId, IEnumerable<int> accessToolIds, DateTime expiredAt, CancellationToken ct)
    {
        try
        {
            foreach (var accessToolId in accessToolIds)
            {
                var result = await GrantAccessToToolAsync(userId, accessToolId, expiredAt, ct);
                if (!result.HasValue)
                {
                    return Option.None<bool, ErrorCustom.Error>(new ErrorCustom.Error("UserAccessTool.GrantAccessFailed", "Failed to grant access", ErrorCustom.ErrorType.Failure));
                }
            }
            return Option.Some<bool, ErrorCustom.Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.GrantMultipleAccessFailed", $"Failed to grant multiple access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<bool, ErrorCustom.Error>> RevokeAllAccessToolsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var userAccessTools = await _userAccessToolRepository.GetByUserIdAsync(userId, ct);
            foreach (var userAccessTool in userAccessTools)
            {
                await _userAccessToolRepository.DeleteAsync(userAccessTool.UserAccessToolId, ct);
            }
            return Option.Some<bool, ErrorCustom.Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.RevokeAllAccessFailed", $"Failed to revoke all access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<bool, ErrorCustom.Error>> UpdateAccessToolsForMembershipAsync(Guid userId, int planId, DateTime membershipExpiryDate, CancellationToken ct)
    {
        try
        {
            // Get the plan to see what access tools should be granted
            var plan = await _membershipPlanRepository.GetPlanByIdAsync(planId, ct);
            if (plan == null)
            {
                return Option.None<bool, ErrorCustom.Error>(
                    new ErrorCustom.Error("UserAccessTool.PlanNotFound", "Membership plan not found", ErrorCustom.ErrorType.NotFound));
            }

            // Parse access tool IDs from plan
            List<int> accessToolIds = new List<int>();

            if (!string.IsNullOrEmpty(plan.AccessToolIds))
            {
                try
                {
                    accessToolIds = JsonSerializer.Deserialize<List<int>>(plan.AccessToolIds) ?? new List<int>();
                }
                catch (JsonException)
                {
                    return Option.None<bool, ErrorCustom.Error>(
                        new ErrorCustom.Error("UserAccessTool.InvalidAccessToolIds", "Invalid access tool IDs format in plan", ErrorCustom.ErrorType.Validation));
                }
            }

            // If no specific access tools defined, grant all tools that don't require membership
            if (!accessToolIds.Any())
            {
                var freeTools = await _accessToolRepository.GetByRequiredMembershipAsync(false, ct);
                accessToolIds = freeTools.Select(t => t.AccessToolId).ToList();
            }

            // Revoke all current access tools
            await RevokeAllAccessToolsAsync(userId, ct);

            // Grant new access tools
            if (accessToolIds.Any())
            {
                var grantResult = await GrantAccessToToolsAsync(userId, accessToolIds, membershipExpiryDate, ct);
                if (!grantResult.HasValue)
                {
                    return Option.None<bool, ErrorCustom.Error>(new ErrorCustom.Error("UserAccessTool.GrantAccessFailed", "Failed to grant access", ErrorCustom.ErrorType.Failure));
                }
            }

            return Option.Some<bool, ErrorCustom.Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.UpdateMembershipAccessFailed", $"Failed to update membership access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}
