namespace CusomMapOSM_Commons.Constant;

public static class FirebaseConstant
{
    private static readonly string CredentialsFileName = "firebase-service-account.json";

    public static string FIREBASE_STORAGE_BUCKET = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET") 
                                                   ?? throw new ApplicationException("Cannot found bucket in environment variables");
    public static string FIREBASE_CREDENTIALS_PATH = ResolveCredentialsPath();
    private static string ResolveCredentialsPath( ) => Path.Combine(GetSolutionRoot(), CredentialsFileName);
    private static string GetSolutionRoot()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }
}