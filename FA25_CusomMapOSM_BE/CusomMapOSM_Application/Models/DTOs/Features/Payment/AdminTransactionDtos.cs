namespace CusomMapOSM_Application.Models.DTOs.Features.Payment;

// Admin transaction list response
public record AdminTransactionListResponse
{
    public required List<AdminTransactionDto> Transactions { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasMore { get; init; }
}

// Individual transaction DTO for admin view
public record AdminTransactionDto
{
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string Description { get; init; }

    public required AdminUserDto User { get; init; }
    public required AdminOrgDto Organization { get; init; }
    public required AdminPlanDto Plan { get; init; }
    public required AdminPaymentGatewayDto PaymentGateway { get; init; }

    public required bool CanDownloadReceipt { get; init; }
}

public record AdminUserDto
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
}

public record AdminOrgDto
{
    public required string OrgId { get; init; }
    public required string OrgName { get; init; }
}

public record AdminPlanDto
{
    public required int PlanId { get; init; }
    public required string PlanName { get; init; }
    public required decimal PriceMonthly { get; init; }
}

public record AdminPaymentGatewayDto
{
    public required string GatewayId { get; init; }
    public required string Name { get; init; }
}

// Transaction statistics
public record TransactionStatistics
{
    public required int TotalCount { get; init; }
    public required decimal TotalRevenue { get; init; }
    public required string Currency { get; init; }
    public required int SuccessCount { get; init; }
    public required int PendingCount { get; init; }
    public required int FailedCount { get; init; }
    public required int CancelledCount { get; init; }
    public required double SuccessRate { get; init; } // Percentage 0-100
}

// Filter request
public record AdminTransactionFilterRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "createdAt";
    public string SortOrder { get; init; } = "desc";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Status { get; init; } // Comma-separated: "Success,Paid"
    public Guid? UserId { get; init; }
    public Guid? OrgId { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string? PaymentGateway { get; init; }
    public string? Search { get; init; }
}

// Bulk download request
public record BulkDownloadReceiptsRequest
{
    public required List<string> TransactionIds { get; init; }
}

// Export request
public record ExportTransactionsRequest
{
    public required string Format { get; init; } // "csv" | "xlsx"
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Status { get; init; }
    public Guid? UserId { get; init; }
    public Guid? OrgId { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string? PaymentGateway { get; init; }
    public string? Search { get; init; }
}
