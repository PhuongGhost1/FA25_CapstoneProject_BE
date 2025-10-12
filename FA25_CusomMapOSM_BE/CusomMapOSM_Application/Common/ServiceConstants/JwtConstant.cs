namespace CusomMapOSM_Application.Common.ServiceConstants;

public static class JwtConstant
{
    public const int ACCESS_TOKEN_EXP = 3600 * 24 * 7; // 7 days
    public const int REFRESH_TOKEN_EXP = 3600 * 24 * 30; // 30 days
    public static string JWT_SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
        throw new ApplicationException("Cannot found jwt key in environment variables");
}
