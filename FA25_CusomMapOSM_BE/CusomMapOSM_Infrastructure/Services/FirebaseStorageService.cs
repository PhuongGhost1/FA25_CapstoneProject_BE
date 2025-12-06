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

    private const string DefaultFolder = "blog";
    private const string CredentialsFileName = "firebase-service-account.json";

    public FirebaseStorageService(IConfiguration configuration)
    {
        _bucketName = GetBucketName(configuration);
        
        InitializeFirebase(configuration);
        
        var credential = GetGoogleCredential(configuration);
        _storageClient = StorageClient.Create(credential);
    }

    #region Initialization

    private string GetBucketName(IConfiguration configuration)
    {
        return configuration["Firebase:StorageBucket"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET")
            ?? throw new InvalidOperationException("Firebase Storage Bucket is not configured");
    }

    private void InitializeFirebase(IConfiguration configuration)
    {
        lock (_firebaseInitLock)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = ResolveCredentialsPath(configuration);
                
                if (string.IsNullOrEmpty(credentialsPath) || !File.Exists(credentialsPath))
                {
                    var searchedPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "firebase-service-account.json"));
                    Console.WriteLine($"[FirebaseStorageService] ERROR: Firebase credentials not found!");
                    Console.WriteLine($"[FirebaseStorageService] Searched at: {searchedPath}");
                    throw new InvalidOperationException(
                        $"Firebase credentials not found. Please set FIREBASE_CREDENTIALS_PATH or place {CredentialsFileName} in solution root.");
                }

                var credential = GoogleCredential.FromFile(credentialsPath);
                FirebaseApp.Create(new AppOptions { Credential = credential });
                _firebaseInitialized = true;
            }
        }
    }

    private GoogleCredential GetGoogleCredential(IConfiguration configuration)
    {
        var credentialsPath = ResolveCredentialsPath(configuration);

        if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
        {
            return GoogleCredential.FromFile(credentialsPath);
        }

        try
        {
            return GoogleCredential.GetApplicationDefault();
        }
        catch
        {
            throw new InvalidOperationException("Failed to initialize Firebase credentials");
        }
    }

    private string ResolveCredentialsPath(IConfiguration configuration)
    {
        var configPath = configuration["Firebase:CredentialsPath"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");

        if (!string.IsNullOrEmpty(configPath))
        {
            return configPath;
        }

        var defaultPath = Path.Combine(GetSolutionRoot(), CredentialsFileName);
        return File.Exists(defaultPath) ? defaultPath : null;
    }

    private static string GetSolutionRoot()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }

    #endregion

    #region Content Type

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

    #endregion

    #region File Operations

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
    {
        return await UploadFileAsync(fileName, fileStream, DefaultFolder);
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string folder)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName, folder);
        var contentType = GetContentType(fileName);

        await _storageClient.UploadObjectAsync(
            bucket: _bucketName,
            objectName: uniqueFileName,
            contentType: contentType,
            source: fileStream);

        return GeneratePublicUrl(uniqueFileName);
    }

    public async Task<string> DownloadFileAsync(string fileName)
    {
        var objectName = ExtractObjectName(fileName);

        try
        {
            await _storageClient.GetObjectAsync(_bucketName, objectName);
            return GeneratePublicUrl(objectName);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found: {fileName}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var objectName = ExtractObjectName(fileName);

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

    #endregion

    #region Helpers

    private string GenerateUniqueFileName(string fileName, string folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{folder}/{Guid.NewGuid()}_{timestamp}_{fileName}";
    }

    private string GeneratePublicUrl(string objectName)
    {
        var encodedName = Uri.EscapeDataString(objectName);
        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedName}?alt=media";
    }

    private static string ExtractObjectName(string fileNameOrUrl)
    {
        if (!fileNameOrUrl.Contains("/o/"))
        {
            return fileNameOrUrl;
        }

        var parts = fileNameOrUrl.Split(new[] { "/o/" }, StringSplitOptions.None);
        if (parts.Length <= 1)
        {
            return fileNameOrUrl;
        }

        var encodedName = parts[1].Split('?')[0];
        return Uri.UnescapeDataString(encodedName);
    }

    #endregion
}