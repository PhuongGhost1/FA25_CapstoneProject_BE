using CusomMapOSM_Application.Interfaces.Services.Firebase;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CusomMapOSM_Infrastructure.Services;

public class FirebaseStorageService : IFirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private static readonly object _firebaseInitLock = new object();
    private static bool _firebaseInitialized = false;

    public FirebaseStorageService(IConfiguration configuration)
    {
        var bucketName = configuration["Firebase:StorageBucket"] 
            ?? Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET") 
            ?? throw new InvalidOperationException("Firebase Storage Bucket is not configured");

        _bucketName = bucketName;
        
        GoogleCredential credential = null;
        
        // Thread-safe initialization of FirebaseApp
        lock (_firebaseInitLock)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = configuration["Firebase:CredentialsPath"] 
                    ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");

                // Tự động tìm file nếu không có đường dẫn
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                    var defaultPath = Path.Combine(solutionRoot, "firebase-service-account.json");
                    
                    if (File.Exists(defaultPath))
                    {
                        credentialsPath = defaultPath;
                    }
                }

                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    credential = GoogleCredential.FromFile(credentialsPath);
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                    _firebaseInitialized = true;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Firebase credentials not found. Please set FIREBASE_CREDENTIALS_PATH or place firebase-service-account.json in solution root. " +
                        $"Searched at: {Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "firebase-service-account.json"))}");
                }
            }
            else
            {
                // FirebaseApp already exists, get credential from file if needed
                var credentialsPath = configuration["Firebase:CredentialsPath"] 
                    ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");
                
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                    var defaultPath = Path.Combine(solutionRoot, "firebase-service-account.json");
                    if (File.Exists(defaultPath))
                    {
                        credentialsPath = defaultPath;
                    }
                }

                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    credential = GoogleCredential.FromFile(credentialsPath);
                }
                else
                {
                    // Fallback to default application credentials
                    try
                    {
                        credential = GoogleCredential.GetApplicationDefault();
                    }
                    catch
                    {
                        throw new InvalidOperationException("Failed to initialize Firebase credentials");
                    }
                }
            }
        }

        // Tạo StorageClient với credentials
        _storageClient = StorageClient.Create(credential);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueFileName = $"blog/{Guid.NewGuid()}_{timestamp}_{fileName}";
        
        var contentType = GetContentType(fileName);
        
        await _storageClient.UploadObjectAsync(
            bucket: _bucketName,
            objectName: uniqueFileName,
            contentType: contentType,
            source: fileStream);
        
        // Get public URL
        var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(uniqueFileName)}?alt=media";
        
        return url;
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueFileName = $"{folder}/{Guid.NewGuid()}_{timestamp}_{fileName}";
        
        var contentType = GetContentType(fileName);
        
        await _storageClient.UploadObjectAsync(
            bucket: _bucketName,
            objectName: uniqueFileName,
            contentType: contentType,
            source: fileStream);
        
        // Get public URL
        var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(uniqueFileName)}?alt=media";
        
        return url;
    }

    public async Task<string> DownloadFileAsync(string fileName)
    {
        // Extract object name from full path if it's a URL
        var objectName = fileName;
        if (fileName.Contains("/o/"))
        {
            var parts = fileName.Split(new[] { "/o/" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var encodedName = parts[1].Split('?')[0];
                objectName = Uri.UnescapeDataString(encodedName);
            }
        }

        try
        {
            var obj = await _storageClient.GetObjectAsync(_bucketName, objectName);
            
            // Generate public URL
            var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media";
            
            return url;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found: {fileName}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var objectName = fileName;
        if (fileName.Contains("/o/"))
        {
            var parts = fileName.Split(new[] { "/o/" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var encodedName = parts[1].Split('?')[0];
                objectName = Uri.UnescapeDataString(encodedName);
            }
        }

        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}

