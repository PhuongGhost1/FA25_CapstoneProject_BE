namespace CusomMapOSM_Commons.Constant;

public static class JwtConstant
{
    public const int ACCESS_TOKEN_EXP = 60 * 60; // 1 hour
    public const int REFRESH_TOKEN_EXP = 3600 * 24 * 30; // 30 days
    public static string JWT_SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
        throw new ApplicationException("Cannot found jwt key in environment variables");
}
