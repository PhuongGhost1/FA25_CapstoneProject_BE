namespace CusomMapOSM_Application.Interfaces.Services.Blog;

public interface IBlogStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream fileStream);
    Task<string> DownloadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}