namespace CusomMapOSM_Application.Models.DTOs.Services;

public class MailRequest
{
    public required string ToEmail { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}