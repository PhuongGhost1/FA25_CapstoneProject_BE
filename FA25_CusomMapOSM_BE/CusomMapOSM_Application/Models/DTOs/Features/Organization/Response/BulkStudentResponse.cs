namespace CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;

public record BulkCreateStudentsResponse
{
    public required int TotalCreated { get; set; }
    public required int TotalSkipped { get; set; }
    public required List<CreatedStudentAccount> CreatedAccounts { get; set; }
    public required List<SkippedStudentAccount> SkippedAccounts { get; set; }
}

public record CreatedStudentAccount
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Password { get; set; }
    public string? Class { get; set; }
}

public record SkippedStudentAccount
{
    public required string Name { get; set; }
    public string? Class { get; set; }
    public required string Reason { get; set; }
}

