using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;

public class UpdateQuestionBankRequest
{
    [Required]
    [StringLength(200)]
    public string BankName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }
    public Guid? WorkspaceId { get; set; }
    public bool IsTemplate { get; set; } = false;
    public bool IsPublic { get; set; } = false;
}