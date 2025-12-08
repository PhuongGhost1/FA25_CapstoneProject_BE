using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.FileUpload;
using CusomMapOSM_Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Infrastructure.Services.FileUpload;

public class FileUploadService : IFileUploadService
{
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserAssetService _userAssetService;

    public FileUploadService(
        IFirebaseStorageService firebaseStorageService,
        ICurrentUserService currentUserService,
        IUserAssetService userAssetService)
    {
        _firebaseStorageService = firebaseStorageService;
        _currentUserService = currentUserService;
        _userAssetService = userAssetService;
    }

    public async Task<Option<string, Error>> UploadFileAsync(
        IFormFile file,
        string[] allowedExtensions,
        string uploadFolder,
        string assetType,
        string invalidTypeErrorMessage,
        Guid? organizationId = null)
    {
        if (file == null || file.Length == 0)
        {
            return Option.None<string, Error>(Error.ValidationError("File.Empty", "No file provided"));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Option.None<string, Error>(Error.ValidationError("File.InvalidType", invalidTypeErrorMessage));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var storageUrl = await _firebaseStorageService.UploadFileAsync(file.FileName, stream, uploadFolder);
            
            // Register in User Library
            var userId = _currentUserService.GetUserId();
            if (userId.HasValue)
            {
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        file.FileName,
                        storageUrl,
                        file.ContentType,
                        file.Length,
                        organizationId);
                }
                catch (Exception) { /* Ensure robust */ }
            }

            return Option.Some<string, Error>(storageUrl);
        }
        catch (Exception ex)
        {
            return Option.None<string, Error>(Error.Failure("File.UploadFailed", ex.Message));
        }
    }
}
