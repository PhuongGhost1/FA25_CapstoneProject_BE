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

                Console.WriteLine($"[FirebaseStorageService] Initial credentials path from config/env: {credentialsPath ?? "null"}");

                // Tự động tìm file nếu không có đường dẫn
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                    var defaultPath = Path.Combine(solutionRoot, "firebase-service-account.json");
                    
                    Console.WriteLine($"[FirebaseStorageService] Base directory: {baseDir}");
                    Console.WriteLine($"[FirebaseStorageService] Solution root: {solutionRoot}");
                    Console.WriteLine($"[FirebaseStorageService] Default path: {defaultPath}");
                    Console.WriteLine($"[FirebaseStorageService] Default path exists: {File.Exists(defaultPath)}");
                    
                    if (File.Exists(defaultPath))
                    {
                        credentialsPath = defaultPath;
                        Console.WriteLine($"[FirebaseStorageService] Using default path: {credentialsPath}");
                    }
                }

                Console.WriteLine($"[FirebaseStorageService] Final credentials path: {credentialsPath ?? "null"}");
                if (!string.IsNullOrEmpty(credentialsPath))
                {
                    Console.WriteLine($"[FirebaseStorageService] Credentials file exists: {File.Exists(credentialsPath)}");
                    if (File.Exists(credentialsPath))
                    {
                        Console.WriteLine($"[FirebaseStorageService] Credentials file full path: {Path.GetFullPath(credentialsPath)}");
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
                    Console.WriteLine($"[FirebaseStorageService] Firebase initialized successfully with credentials: {Path.GetFullPath(credentialsPath)}");
                }
                else
                {
                    var searchedPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "firebase-service-account.json"));
                    Console.WriteLine($"[FirebaseStorageService] ERROR: Firebase credentials not found!");
                    Console.WriteLine($"[FirebaseStorageService] Searched at: {searchedPath}");
                    throw new InvalidOperationException(
                        $"Firebase credentials not found. Please set FIREBASE_CREDENTIALS_PATH or place firebase-service-account.json in solution root. " +
                        $"Searched at: {searchedPath}");
                }
            }
            else
            {
                // FirebaseApp already exists, get credential from file if needed
                var credentialsPath = configuration["Firebase:CredentialsPath"] 
                    ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");
                
                Console.WriteLine($"[FirebaseStorageService] FirebaseApp already exists. Getting credentials from: {credentialsPath ?? "null"}");
                
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                    var defaultPath = Path.Combine(solutionRoot, "firebase-service-account.json");
                    Console.WriteLine($"[FirebaseStorageService] Checking default path: {defaultPath}");
                    Console.WriteLine($"[FirebaseStorageService] Default path exists: {File.Exists(defaultPath)}");
                    if (File.Exists(defaultPath))
                    {
                        credentialsPath = defaultPath;
                        Console.WriteLine($"[FirebaseStorageService] Using default path: {credentialsPath}");
                    }
                }

                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    Console.WriteLine($"[FirebaseStorageService] Loading credentials from: {Path.GetFullPath(credentialsPath)}");
                    credential = GoogleCredential.FromFile(credentialsPath);
                }
                else
                {
                    // Fallback to default application credentials
                    Console.WriteLine($"[FirebaseStorageService] Credentials file not found, trying default application credentials");
                    try
                    {
                        credential = GoogleCredential.GetApplicationDefault();
                        Console.WriteLine($"[FirebaseStorageService] Successfully loaded default application credentials");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FirebaseStorageService] ERROR: Failed to load default application credentials: {ex.Message}");
                        throw new InvalidOperationException("Failed to initialize Firebase credentials", ex);
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

    public async Task<string?> FindFileByPatternAsync(string folder, string fileNamePattern)
    {
        try
        {
            // Ensure folder ends with / for proper prefix matching
            var searchPrefix = folder.EndsWith("/") ? folder : folder + "/";
            
            // List all objects in the folder
            // PagedEnumerable is enumerable synchronously, not asynchronously
            var objects = _storageClient.ListObjects(_bucketName, searchPrefix);
            
            int count = 0;
            foreach (var obj in objects)
            {
                count++;
                // Check if the object name contains the filename pattern
                // Firebase stores files as: {folder}/{guid}_{timestamp}_{filename}
                // So we search for files containing the filename
                if (obj.Name.Contains(fileNamePattern, StringComparison.OrdinalIgnoreCase))
                {
                    // Generate public URL for the found file
                    var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(obj.Name)}?alt=media";
                    return url;
                }
            }
            
            // If no files found, return null
            return null;
        }
        catch (Google.GoogleApiException ex)
        {
            // Log the error for debugging - this might indicate permission issues
            throw new InvalidOperationException(
                $"Firebase Storage API error while searching for file in folder '{folder}' with pattern '{fileNamePattern}': {ex.Message}. " +
                $"Status Code: {ex.HttpStatusCode}. Make sure Firebase credentials are properly configured and have Storage access permissions.", ex);
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            throw new InvalidOperationException(
                $"Error while searching for file in Firebase Storage folder '{folder}' with pattern '{fileNamePattern}': {ex.Message}", ex);
        }
    }
}

