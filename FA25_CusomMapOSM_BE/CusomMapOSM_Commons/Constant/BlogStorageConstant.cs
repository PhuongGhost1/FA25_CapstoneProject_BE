namespace CusomMapOSM_Commons.Constant;

public static class BlogStorageConstant
{
    public static string BLOG_STORAGE_CONNECTION_STRING = Environment.GetEnvironmentVariable("BLOG_STORAGE_CONNECTION_STRING") ??
        throw new ApplicationException("Cannot found blog storage connection string in environment variables");
    
    public static string BLOG_STORAGE_CONTAINER_NAME = Environment.GetEnvironmentVariable("BLOG_STORAGE_CONTAINER_NAME") ?? "blog-files";
}