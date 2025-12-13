using System;
using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.OrganizationAdmin;

// Organization Usage DTOs
public record OrganizationUsageResponse
{
    public required Guid OrgId { get; set; }
    public required string OrganizationName { get; set; }
    public required IReadOnlyList<UsageQuotaDto> AggregatedQuotas { get; set; }
    public required IReadOnlyList<UserUsageSummaryDto> UserUsageSummaries { get; set; }
    public DateTime LastResetDate { get; set; }
    public DateTime NextResetDate { get; set; }
    public int TotalActiveUsers { get; set; }
    public int TotalMapsCreated { get; set; }
    public int TotalExportsThisMonth { get; set; }
}

public record UsageQuotaDto
{
    public required string ResourceType { get; set; }
    public int CurrentUsage { get; set; }
    public int Limit { get; set; }
    public bool IsExceeded => CurrentUsage >= Limit && Limit != -1;
    public int Remaining => Limit == -1 ? int.MaxValue : Limit - CurrentUsage;
    public double UsagePercentage => Limit == -1 ? 0 : (double)CurrentUsage / Limit * 100;
}

public record UserUsageSummaryDto
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string Role { get; set; }
    public required IReadOnlyList<UsageQuotaDto> Quotas { get; set; }
    public DateTime LastActive { get; set; }
}

// Organization Subscription DTOs
public record OrganizationSubscriptionResponse
{
    public required Guid OrgId { get; set; }
    public required string OrganizationName { get; set; }
    public required IReadOnlyList<MembershipSummaryDto> ActiveMemberships { get; set; }
    public required IReadOnlyList<MembershipSummaryDto> PendingMemberships { get; set; }
    public required IReadOnlyList<MembershipSummaryDto> ExpiredMemberships { get; set; }
    public DateTime NextBillingDate { get; set; }
    public decimal TotalMonthlyCost { get; set; }
    public string PrimaryPlanName { get; set; } = string.Empty;
    public bool HasActiveSubscription { get; set; }
}

public record MembershipSummaryDto
{
    public required Guid MembershipId { get; set; }
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required int PlanId { get; set; }
    public required string PlanName { get; set; }
    public required string Status { get; set; }
    public DateTime BillingCycleStartDate { get; set; }
    public DateTime BillingCycleEndDate { get; set; }
    public bool AutoRenew { get; set; }
    public decimal MonthlyCost { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Organization Billing DTOs
public record OrganizationBillingResponse
{
    public required Guid OrgId { get; set; }
    public required string OrganizationName { get; set; }
    public required IReadOnlyList<BillingTransactionDto> RecentTransactions { get; set; }
    public required IReadOnlyList<BillingInvoiceDto> RecentInvoices { get; set; }
    public decimal TotalSpentThisMonth { get; set; }
    public decimal TotalSpentLastMonth { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime NextBillingDate { get; set; }
    public bool HasPaymentMethodOnFile { get; set; }
}

public record BillingTransactionDto
{
    public required Guid TransactionId { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public required string PaymentGateway { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? GatewayTransactionId { get; set; }
}

public record BillingInvoiceDto
{
    public required string InvoiceNumber { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? DownloadUrl { get; set; }
}

// Organization Member Management DTOs
public record OrganizationMemberDto
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string Role { get; set; }
    public required string Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastActive { get; set; }
    public bool IsActive { get; set; }
}

public record UpdateMemberRoleRequest
{
    public required Guid UserId { get; set; }
    public required string NewRole { get; set; }
}

public record UpdateMemberRoleResponse
{
    public required Guid UserId { get; set; }
    public required string OldRole { get; set; }
    public required string NewRole { get; set; }
    public required string Message { get; set; }
}

public record RemoveMemberRequest
{
    public required Guid UserId { get; set; }
    public string? Reason { get; set; }
}

public record RemoveMemberResponse
{
    public required Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string Message { get; set; }
}
