using CusomMapOSM_Application.Models.DTOs.Services.MinIO;

namespace CusomMapOSM_Application.Interfaces.Services.MinIO;

public interface IMinIOService
{
    Task<string> UploadFileAsync(
        Stream stream, 
        string fileName, 
        string contentType, 
        string? folder = null,
        CancellationToken ct = default);
    
    Task<(Stream stream, string contentType)> DownloadFileAsync(
        string objectName, 
        CancellationToken ct = default);

    Task<bool> DeleteFileAsync(
        string objectName, 
        CancellationToken ct = default);
    
    Task<int> BulkDeleteFilesAsync(
        List<string> objectNames, 
        CancellationToken ct = default);
    
    Task<string> GetPresignedUrlAsync(
        string objectName, 
        int expiryInSeconds = 3600,
        CancellationToken ct = default);
    
    Task<bool> FileExistsAsync(
        string objectName, 
        CancellationToken ct = default);

    Task<MinIOFileMetadata?> GetFileMetadataAsync(
        string objectName, 
        CancellationToken ct = default);
    
    Task<bool> CopyFileAsync(
        string sourceObjectName, 
        string destinationObjectName, 
        CancellationToken ct = default);

    Task<List<string>> ListFilesAsync(
        string folderPrefix, 
        CancellationToken ct = default);
    
    Task<long> GetFolderSizeAsync(
        string folderPrefix, 
        CancellationToken ct = default);
}