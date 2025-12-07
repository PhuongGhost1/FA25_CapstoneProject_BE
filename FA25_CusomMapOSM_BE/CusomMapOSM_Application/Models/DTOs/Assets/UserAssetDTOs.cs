using System;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Models.DTOs.Assets;

public record UserAssetDto(
    Guid Id,
    string Name,
    string Url,
    string Type,
    long Size,
    DateTime CreatedAt
);

public class UploadAssetRequest
{
    public IFormFile File { get; set; } = null!;
    // "image" or "audio" - can be inferred or explicit
    public string Type { get; set; } = "image"; 
}
