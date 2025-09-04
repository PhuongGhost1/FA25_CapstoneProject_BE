namespace CusomMapOSM_Application.Common.ServiceConstants;

public static class MySqlDatabase
{
    public static string CONNECTION_STRING = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
        throw new ApplicationException("Cannot found mysql connection string in environment variables");
}