using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Services.UserAsset.Request;
using CusomMapOSM_Application.Models.DTOs.Services.UserAsset.Response;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Services.Assets;

public interface IUserAssetService
{
    Task<Option<UserAssetListResponse, Error>>  GetUserAssetsAsync(Guid? orgId = null, string? type = null, int page = 1, int pageSize = 20);
    Task<UserAssetRequest> UploadAssetAsync(IFormFile file, Guid? orgId = null);

    Task<UserAssetRequest> CreateAssetMetadataAsync(string name, string url, string contentType, long size,
        Guid? orgId = null);
    Task DeleteAssetAsync(Guid userId, Guid assetId);
}
