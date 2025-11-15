namespace CusomMapOSM_Commons.Constant;

public class FrontendConstant
{
    public static readonly string FRONTEND_ORIGINS = Environment.GetEnvironmentVariable("FRONTEND_ORIGINS") ??
                                                     throw new ApplicationException("Cannot found frontend url in environment variables");
}