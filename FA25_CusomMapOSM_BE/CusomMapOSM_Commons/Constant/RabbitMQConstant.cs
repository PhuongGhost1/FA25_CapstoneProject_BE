namespace CusomMapOSM_Commons.Constant;
public static class RabbitMQConstant
{
    public static string Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    public static int Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672;
    public static string Username = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin";
    public static string Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin123";
    public static string VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";
    
    // Queue names
    public static string EmailQueue = "email_queue";
    public static string EmailDeadLetterQueue = "email_dead_letter_queue";
    public static string EmailExchange = "email_exchange";
}