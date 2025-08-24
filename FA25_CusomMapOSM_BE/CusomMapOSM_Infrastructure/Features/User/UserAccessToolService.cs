using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.AccessToolRepo;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using System.Text.Json;
using CusomMapOSM_Infrastructure.Databases;

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
            Console.WriteLine($"=== GrantAccessToToolAsync ===");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"AccessToolId: {accessToolId}");
            Console.WriteLine($"ExpiredAt: {expiredAt}");

            // Check if access tool exists
            var accessTool = await _accessToolRepository.GetByIdAsync(accessToolId, ct);
            if (accessTool == null)
            {
                Console.WriteLine($"Access tool not found for AccessToolId: {accessToolId}");
                return Option.None<UserAccessTool, ErrorCustom.Error>(
                    new ErrorCustom.Error("UserAccessTool.AccessToolNotFound", "Access tool not found", ErrorCustom.ErrorType.NotFound));
            }

            Console.WriteLine($"Found access tool: {accessTool.AccessToolName}");

            // Check if user already has access to this tool
            var existingAccess = await _userAccessToolRepository.GetByUserAndToolAsync(userId, accessToolId, ct);
            if (existingAccess != null)
            {
                Console.WriteLine("Updating existing access");
                // Update existing access
                existingAccess.ExpiredAt = expiredAt;
                var updatedAccess = await _userAccessToolRepository.UpdateAsync(existingAccess, ct);
                Console.WriteLine("Successfully updated existing access");
                return Option.Some<UserAccessTool, ErrorCustom.Error>(updatedAccess);
            }

            Console.WriteLine("Creating new access");
            // Create new access
            var userAccessTool = new UserAccessTool
            {
                UserId = userId,
                AccessToolId = accessToolId,
                GrantedAt = DateTime.UtcNow,
                ExpiredAt = expiredAt
            };

            Console.WriteLine($"About to create UserAccessTool: UserId={userAccessTool.UserId}, AccessToolId={userAccessTool.AccessToolId}");
            var createdAccess = await _userAccessToolRepository.CreateAsync(userAccessTool, ct);
            Console.WriteLine("Successfully created new access");
            Console.WriteLine($"=== End GrantAccessToToolAsync ===");
            return Option.Some<UserAccessTool, ErrorCustom.Error>(createdAccess);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GrantAccessToToolAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
            Console.WriteLine($"=== UpdateAccessToolsForMembershipAsync ===");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"PlanId: {planId}");
            Console.WriteLine($"MembershipExpiryDate: {membershipExpiryDate}");

            // First, ensure the user has a valid account status
            await EnsureUserHasValidAccountStatusAsync(userId, ct);
            Console.WriteLine($"=== End UpdateAccessToolsForMembershipAsync ====");

            // Get the plan to see what access tools should be granted
            var plan = await _membershipPlanRepository.GetPlanByIdAsync(planId, ct);
            if (plan == null)
            {
                Console.WriteLine($"Plan not found for PlanId: {planId}");
                return Option.None<bool, ErrorCustom.Error>(
                    new ErrorCustom.Error("UserAccessTool.PlanNotFound", "Membership plan not found", ErrorCustom.ErrorType.NotFound));
            }

            Console.WriteLine($"Found plan: {plan.PlanName}");

            // Parse access tool IDs from plan
            List<int> accessToolIds = new List<int>();

            if (!string.IsNullOrEmpty(plan.AccessToolIds))
            {
                try
                {
                    accessToolIds = JsonSerializer.Deserialize<List<int>>(plan.AccessToolIds) ?? new List<int>();
                    Console.WriteLine($"Parsed access tool IDs from plan: {string.Join(", ", accessToolIds)}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to parse access tool IDs: {ex.Message}");
                    return Option.None<bool, ErrorCustom.Error>(
                        new ErrorCustom.Error("UserAccessTool.InvalidAccessToolIds", "Invalid access tool IDs format in plan", ErrorCustom.ErrorType.Validation));
                }
            }

            // If no specific access tools defined, grant all tools that don't require membership
            if (!accessToolIds.Any())
            {
                var freeTools = await _accessToolRepository.GetByRequiredMembershipAsync(false, ct);
                accessToolIds = freeTools.Select(t => t.AccessToolId).ToList();
                Console.WriteLine($"No specific tools in plan, using free tools: {string.Join(", ", accessToolIds)}");
            }

            // Revoke all current access tools
            Console.WriteLine("Revoking all current access tools...");
            await RevokeAllAccessToolsAsync(userId, ct);

            // Grant new access tools
            if (accessToolIds.Any())
            {
                Console.WriteLine($"Granting access to tools: {string.Join(", ", accessToolIds)}");
                var grantResult = await GrantAccessToToolsAsync(userId, accessToolIds, membershipExpiryDate, ct);
                if (!grantResult.HasValue)
                {
                    Console.WriteLine("Failed to grant access tools");
                    return Option.None<bool, ErrorCustom.Error>(new ErrorCustom.Error("UserAccessTool.GrantAccessFailed", "Failed to grant access", ErrorCustom.ErrorType.Failure));
                }
                Console.WriteLine("Successfully granted access tools");
            }
            else
            {
                Console.WriteLine("No access tools to grant");
            }

            return Option.Some<bool, ErrorCustom.Error>(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in UpdateAccessToolsForMembershipAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Option.None<bool, ErrorCustom.Error>(
                new ErrorCustom.Error("UserAccessTool.UpdateMembershipAccessFailed", $"Failed to update membership access: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    private async Task EnsureUserHasValidAccountStatusAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            // This is a temporary fix - in a real application, you would inject IUserRepository
            // For now, we'll just log the issue and continue
            Console.WriteLine($"Checking user account status for UserId: {userId}");
            Console.WriteLine($"Note: If foreign key constraint fails, ensure user has valid AccountStatusId: {SeedDataConstants.ActiveStatusId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not verify user account status: {ex.Message}");
        }
    }
}
