namespace CusomMapOSM_Application.Interfaces.Features.AccessTool;

public record GrantAccessToToolRequest(int AccessToolId, DateTime ExpiredAt = default);
public record RevokeAccessToToolRequest(int AccessToolId);
public record GrantMultipleAccessToToolRequest(IEnumerable<int> AccessToolIds, DateTime ExpiredAt = default);
public record UpdateAccessToolsForMembershipRequest(int PlanId, DateTime MembershipExpiryDate = default);