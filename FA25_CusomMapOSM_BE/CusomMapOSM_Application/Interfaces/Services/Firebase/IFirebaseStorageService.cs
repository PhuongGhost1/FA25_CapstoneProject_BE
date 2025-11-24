using System.IO;

namespace CusomMapOSM_Application.Interfaces.Services.Firebase;

public interface IFirebaseStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream fileStream);
    Task<string> UploadFileAsync(string fileName, Stream fileStream, string folder);
    Task<string> DownloadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}

