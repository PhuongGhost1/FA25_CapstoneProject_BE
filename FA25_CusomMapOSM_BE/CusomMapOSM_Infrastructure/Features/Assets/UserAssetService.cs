using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Services.UserAsset.Request;
using CusomMapOSM_Application.Models.DTOs.Services.UserAsset.Response;
using CusomMapOSM_Infrastructure.Services.UserAssets.Mongo;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Assets;

public class UserAssetService : IUserAssetService
{
    private readonly MongoUserAssetStore _mongoStore;
    private readonly IFirebaseStorageService _firebaseStorage;
    private readonly ICurrentUserService _currentUserService;

    public UserAssetService(MongoUserAssetStore mongoStore, IFirebaseStorageService firebaseStorage,
        ICurrentUserService currentUserService)
    {
        _mongoStore = mongoStore;
        _firebaseStorage = firebaseStorage;
        _currentUserService = currentUserService;
    }


    public async Task<Option<UserAssetListResponse, Error>> GetUserAssetsAsync(Guid? orgId = null, string? type = null, int page = 1, int pageSize = 20)
    {
        var userId = _currentUserService.GetUserId().Value;
        var (docs, totalCount) = await _mongoStore.GetAssetsAsync(userId, orgId, type, page, pageSize);
    
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        var items = docs.Select(d => new UserAssetResponse
        {
            Id = d.Id,
            Name = d.Name,
            Url = d.Url,
            Type = d.Type,
            Size = d.Size,
            CreatedAt = d.CreatedAt
        }).ToList();

        return Option.Some<UserAssetListResponse, Error>(new UserAssetListResponse
        {
            Assets = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }


    public async Task<UserAssetRequest> UploadAssetAsync(IFormFile file, Guid? orgId = null)
    {
        var userId = _currentUserService.GetUserId().Value;
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }
        
        var folder = $"users/{userId}/{file.ContentType}s";
        using var stream = file.OpenReadStream();
        var url = await _firebaseStorage.UploadFileAsync(file.FileName, stream, folder);
        
        return await CreateAssetMetadataAsync(
            file.FileName,
            url,
            file.ContentType,
            file.Length,
            orgId);
    }

    public async Task<UserAssetRequest> CreateAssetMetadataAsync(string name, string url, string contentType, long size,
        Guid? orgId = null)
    {
        var userId = _currentUserService.GetUserId().Value;
        var asset = new UserAssetBsonDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = orgId,
            Name = name,
            Url = url,
            Type = contentType,
            ContentType = contentType,
            Size = size,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoStore.AddAsync(asset);

        return new UserAssetRequest(
            asset.Id,
            asset.Name,
            asset.Url,
            asset.Type,
            asset.Size,
            asset.CreatedAt
        );
    }

    public async Task DeleteAssetAsync(Guid userId, Guid assetId)
    {
        var asset = await _mongoStore.GetByIdAsync(assetId);
        if (asset == null)
        {
            return; // Or throw NotFound
        }

        if (asset.UserId != userId)
        {
            throw new UnauthorizedAccessException("Cannot delete asset of another user");
        }

        // Delete from Firebase (optimistic, don't fail if already gone)
        try
        {
            await _firebaseStorage.DeleteFileAsync(asset.Url);
        }
        catch
        {
            // Log warning
        }

        // Delete from Mongo
        await _mongoStore.DeleteAsync(assetId);
    }
}