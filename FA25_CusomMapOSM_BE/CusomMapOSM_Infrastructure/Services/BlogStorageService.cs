using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CusomMapOSM_Application.Interfaces.Services.Blog;
using CusomMapOSM_Commons.Constant;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Services;

public class BlogStorageService : IBlogStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlogStorageService> _logger;
    private readonly string _containerName;

    public BlogStorageService(ILogger<BlogStorageService> logger)
    {
        _logger = logger;
        _containerName = BlogStorageConstant.BLOG_STORAGE_CONTAINER_NAME;
        
        try
        {
            _blobServiceClient = new BlobServiceClient(BlogStorageConstant.BLOG_STORAGE_CONNECTION_STRING);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Ensure container exists with public blob access so images can be displayed directly
            _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob).Wait();
            
            _logger.LogInformation("BlogStorageService initialized with container: {ContainerName}", _containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize BlogStorageService");
            throw;
        }
    }

    /// <summary>
    /// Gets the MIME type based on file extension
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            // Images
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            
            // Documents
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            
            // Text
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            
            // Archives
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            
            // Video
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".webm" => "video/webm",
            
            // Audio
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            
            // Default
            _ => "application/octet-stream"
        };
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
    {
        try
        {
            // Generate unique filename with timestamp to avoid conflicts
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueFileName = $"{Guid.NewGuid()}_{timestamp}_{fileName}";
            
            var blobClient = _containerClient.GetBlobClient(uniqueFileName);
            
            // Detect content type from file extension
            var contentType = GetContentType(fileName);
            
            // Set blob HTTP headers with correct content type
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "public, max-age=31536000" // Cache for 1 year
            };
            
            // Upload options with headers
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            };
            
            // Upload the file stream with headers
            await blobClient.UploadAsync(fileStream, uploadOptions);
            
            // Get the blob URL
            var blobUrl = blobClient.Uri.ToString();
            
            _logger.LogInformation("File uploaded successfully: {FileName} -> {BlobUrl} (ContentType: {ContentType})", 
                fileName, blobUrl, contentType);
            
            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> DownloadFileAsync(string fileName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            
            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("File not found in Azure Blob Storage: {FileName}", fileName);
                throw new FileNotFoundException($"File not found: {fileName}");
            }
            
            // Get the blob URL (signed URL or public URL)
            var blobUrl = blobClient.Uri.ToString();
            
            _logger.LogInformation("File download URL generated: {FileName} -> {BlobUrl}", fileName, blobUrl);
            
            return blobUrl;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            
            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("File not found for deletion: {FileName}", fileName);
                return false;
            }
            
            // Delete the blob
            var result = await blobClient.DeleteIfExistsAsync();
            
            if (result.Value)
            {
                _logger.LogInformation("File deleted successfully: {FileName}", fileName);
                return true;
            }
            
            _logger.LogWarning("File deletion returned false: {FileName}", fileName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FileName}", fileName);
            throw;
        }
    }
}
