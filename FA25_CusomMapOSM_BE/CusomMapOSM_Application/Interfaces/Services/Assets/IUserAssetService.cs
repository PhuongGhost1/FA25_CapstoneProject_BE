using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CusomMapOSM_Application.Models.DTOs.Assets;

namespace CusomMapOSM_Application.Interfaces.Services.Assets;

public interface IUserAssetService
{
    Task<List<UserAssetDto>> GetUserAssetsAsync(Guid userId, Guid? orgId = null, string? type = null);
    Task<UserAssetDto> UploadAssetAsync(Guid userId, UploadAssetRequest request, Guid? orgId = null);
    Task<UserAssetDto> CreateAssetMetadataAsync(Guid userId, string name, string url, string type, long size, string contentType, Guid? orgId = null);
    Task DeleteAssetAsync(Guid userId, Guid assetId);
}
