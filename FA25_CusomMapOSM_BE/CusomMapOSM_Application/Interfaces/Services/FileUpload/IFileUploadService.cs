using CusomMapOSM_Application.Common.Errors;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Services.FileUpload;

public interface IFileUploadService
{
    Task<Option<string, Error>> UploadFileAsync(
        IFormFile file,
        string[] allowedExtensions,
        string uploadFolder,
        string assetType,
        string invalidTypeErrorMessage,
        Guid? organizationId = null);
}
