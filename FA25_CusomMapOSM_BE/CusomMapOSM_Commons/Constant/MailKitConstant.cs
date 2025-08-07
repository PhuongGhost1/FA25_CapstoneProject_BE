namespace CusomMapOSM_Shared.Constant;

public static class MailKitConstant
{
    public static string SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ??
        throw new ArgumentNullException("SMTP_SERVER", "SMTP_SERVER is not set in the environment variables.");
    public static int SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var smtpPort) ? smtpPort :
        throw new ArgumentNullException("SMTP_PORT", "SMTP_PORT is not set in the environment variables.");
    public static string SmtpUsername = Environment.GetEnvironmentVariable("SMTP_USER") ??
        throw new ArgumentNullException("SMTP_USERNAME", "SMTP_USERNAME is not set in the environment variables.");
    public static string SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASS") ??
        throw new ArgumentNullException("SMTP_PASSWORD", "SMTP_PASSWORD is not set in the environment variables.");
}