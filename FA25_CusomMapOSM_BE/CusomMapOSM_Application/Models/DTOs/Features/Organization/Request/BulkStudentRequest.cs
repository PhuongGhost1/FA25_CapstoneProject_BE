namespace CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;

public record BulkCreateStudentsRequest
{
    public required Guid OrganizationId { get; set; }
    public required string Domain { get; set; } // Domain for email generation (e.g., "school.edu")
}

public record StudentAccountInfo
{
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Password { get; set; }
    public string? Class { get; set; }
}

