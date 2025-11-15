namespace CusomMapOSM_Commons.Constant;

public class RedisConstant
{
    public static readonly string REDIS_CONNECTION_STRING = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ??
                                                            throw new ApplicationException("Cannot found redis connection string in environment variables");
}