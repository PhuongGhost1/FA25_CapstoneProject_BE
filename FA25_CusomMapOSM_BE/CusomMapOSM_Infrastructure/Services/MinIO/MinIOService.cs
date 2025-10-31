using CusomMapOSM_Application.Interfaces.Services.MinIO;
using CusomMapOSM_Application.Models.DTOs.Services.MinIO;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio.ApiEndpoints;

namespace CusomMapOSM_Infrastructure.Services.MinIO;


public class MinIOService : IMinIOService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinIOService> _logger;
    private readonly string _bucketName;

    public MinIOService(
        IMinioClient minioClient,
        IConfiguration configuration,
        ILogger<MinIOService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
        _bucketName = configuration["MinIO:BucketName"] ?? "custommaposm-media";

        EnsureBucketExistsAsync().Wait();
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);
            
            bool found = await _minioClient.BucketExistsAsync(beArgs);

            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
                
                _logger.LogInformation("Created MinIO bucket: {BucketName}", _bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring MinIO bucket exists: {BucketName}", _bucketName);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken ct = default)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueFileName = $"{Guid.NewGuid()}_{timestamp}_{fileName}";
            var objectName = string.IsNullOrEmpty(folder)
                ? uniqueFileName
                : $"{folder.TrimEnd('/')}/{uniqueFileName}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, ct);

            _logger.LogInformation("Uploaded file to MinIO: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to MinIO: {FileName}", fileName);
            throw;
        }
    }

    public async Task<(Stream stream, string contentType)> DownloadFileAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var memoryStream = new MemoryStream();
            string contentType = "application/octet-stream";

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream((stream) =>
                {
                    stream.CopyTo(memoryStream);
                });

            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            var stat = await _minioClient.StatObjectAsync(statObjectArgs, ct);
            contentType = stat.ContentType;

            await _minioClient.GetObjectAsync(getObjectArgs, ct);
            memoryStream.Position = 0;

            return (memoryStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from MinIO: {ObjectName}", objectName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, ct);

            _logger.LogInformation("Deleted file from MinIO: {ObjectName}", objectName);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            _logger.LogWarning("File not found in MinIO: {ObjectName}", objectName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from MinIO: {ObjectName}", objectName);
            return false;
        }
    }

    public async Task<int> BulkDeleteFilesAsync(
        List<string> objectNames,
        CancellationToken ct = default)
    {
        int deletedCount = 0;

        foreach (var objectName in objectNames)
        {
            if (await DeleteFileAsync(objectName, ct))
            {
                deletedCount++;
            }
        }

        _logger.LogInformation("Bulk deleted {DeletedCount}/{TotalCount} files from MinIO", 
            deletedCount, objectNames.Count);

        return deletedCount;
    }

    public async Task<string> GetPresignedUrlAsync(
        string objectName,
        int expiryInSeconds = 3600,
        CancellationToken ct = default)
    {
        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithExpiry(expiryInSeconds);

            var url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

            _logger.LogDebug("Generated presigned URL for: {ObjectName}", objectName);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for: {ObjectName}", objectName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.StatObjectAsync(statObjectArgs, ct);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {ObjectName}", objectName);
            return false;
        }
    }

    public async Task<MinIOFileMetadata?> GetFileMetadataAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            var stat = await _minioClient.StatObjectAsync(statObjectArgs, ct);

            return new MinIOFileMetadata
            {
                ObjectName = objectName,
                Size = stat.Size,
                ContentType = stat.ContentType,
                LastModified = stat.LastModified,
                ETag = stat.ETag
            };
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata: {ObjectName}", objectName);
            return null;
        }
    }

    public async Task<bool> CopyFileAsync(
        string sourceObjectName,
        string destinationObjectName,
        CancellationToken ct = default)
    {
        try
        {
            var copySourceObjectArgs = new CopySourceObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(sourceObjectName);

            var copyObjectArgs = new CopyObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(destinationObjectName)
                .WithCopyObjectSource(copySourceObjectArgs);

            await _minioClient.CopyObjectAsync(copyObjectArgs, ct);

            _logger.LogInformation("Copied file in MinIO: {Source} -> {Destination}", 
                sourceObjectName, destinationObjectName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying file in MinIO: {Source} -> {Destination}", 
                sourceObjectName, destinationObjectName);
            return false;
        }
    }

    public async Task<List<string>> ListFilesAsync(
        string folderPrefix,
        CancellationToken ct = default)
    {
        try
        {
            var fileNames = new List<string>();

            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(folderPrefix)
                .WithRecursive(true);

            await foreach (var item in _minioClient.ListObjectsEnumAsync(listObjectsArgs, ct))
            {
                if (!item.IsDir)
                {
                    fileNames.Add(item.Key);
                }
            }

            return fileNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in MinIO folder: {FolderPrefix}", folderPrefix);
            return new List<string>();
        }
    }

    public async Task<long> GetFolderSizeAsync(
        string folderPrefix,
        CancellationToken ct = default)
    {
        try
        {
            long totalSize = 0;

            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(folderPrefix)
                .WithRecursive(true);

            await foreach (var item in _minioClient.ListObjectsEnumAsync(listObjectsArgs, ct))
            {
                if (!item.IsDir)
                {
                    totalSize += (long)item.Size;
                }
            }

            return totalSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder size: {FolderPrefix}", folderPrefix);
            return 0;
        }
    }
}
