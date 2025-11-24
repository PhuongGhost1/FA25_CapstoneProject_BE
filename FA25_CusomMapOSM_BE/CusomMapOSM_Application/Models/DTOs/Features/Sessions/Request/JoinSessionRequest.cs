using System.ComponentModel.DataAnnotations;

namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;

public class JoinSessionRequest
{
    [Required]
    [StringLength(10)]
    public string SessionCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }
}