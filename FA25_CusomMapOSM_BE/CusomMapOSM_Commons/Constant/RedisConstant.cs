namespace CusomMapOSM_Commons.Constant;

public static class RedisConstant
{
    public static string REDIS_CONNECTION_STRING = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ??
        throw new ApplicationException("Cannot found redis connection string in environment variables");
}