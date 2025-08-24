using System;

namespace CusomMapOSM_Application.Models.DTOs.Features.Membership;

public record CreateMembershipRequest(Guid UserId, Guid OrgId, int PlanId, bool AutoRenew);
public record CreateMembershipResponse(Guid MembershipId);

public record PurchaseAddonRequest(Guid MembershipId, Guid OrgId, string AddonKey, int? Quantity, bool EffectiveImmediately);
public record PurchaseAddonResponse(Guid AddonId, string Status);

public record TrackUsageRequest(Guid MembershipId, Guid OrgId, string ResourceKey, int Amount);
public record TrackUsageResponse(bool Success);

public record FeatureCheckResponse(bool HasFeature);

// New DTOs for subscription plan changes
public record ChangeSubscriptionPlanRequest(Guid UserId, Guid OrgId, int NewPlanId, bool AutoRenew);
public record ChangeSubscriptionPlanResponse(Guid MembershipId, string Status, decimal? ProRatedAmount, DateTime EffectiveDate);


