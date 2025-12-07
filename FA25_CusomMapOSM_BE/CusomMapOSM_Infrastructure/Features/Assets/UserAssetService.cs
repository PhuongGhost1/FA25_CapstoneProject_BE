using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Models.DTOs.Assets;
using CusomMapOSM_Infrastructure.Services.UserAssets.Mongo;

namespace CusomMapOSM_Infrastructure.Features.Assets;

public class UserAssetService : IUserAssetService
{
    private readonly MongoUserAssetStore _mongoStore;
    private readonly IFirebaseStorageService _firebaseStorage;

    public UserAssetService(MongoUserAssetStore mongoStore, IFirebaseStorageService firebaseStorage)
    {
        _mongoStore = mongoStore;
        _firebaseStorage = firebaseStorage;
    }

    public async Task<List<UserAssetDto>> GetUserAssetsAsync(Guid userId, Guid? orgId = null, string? type = null)
    {
        var docs = await _mongoStore.GetAssetsAsync(userId, orgId, type);
        return docs.Select(d => new UserAssetDto(
            d.Id,
            d.Name,
            d.Url,
            d.Type,
            d.Size,
            d.CreatedAt
        )).ToList();
    }

    public async Task<UserAssetDto> UploadAssetAsync(Guid userId, UploadAssetRequest request, Guid? orgId = null)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        // Upload to Firebase
        var folder = $"users/{userId}/{request.Type}s"; // users/{guid}/images or users/{guid}/audios
        using var stream = request.File.OpenReadStream();
        var url = await _firebaseStorage.UploadFileAsync(request.File.FileName, stream, folder);

        // Save metadata to Mongo
        return await CreateAssetMetadataAsync(
            userId, 
            request.File.FileName, 
            url, 
            request.Type, 
            request.File.Length, 
            request.File.ContentType, 
            orgId);
    }

    public async Task<UserAssetDto> CreateAssetMetadataAsync(Guid userId, string name, string url, string type, long size, string contentType, Guid? orgId = null)
    {
        var asset = new UserAssetBsonDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = orgId,
            Name = name,
            Url = url,
            Type = type,
            ContentType = contentType,
            Size = size,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoStore.AddAsync(asset);

        return new UserAssetDto(
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
