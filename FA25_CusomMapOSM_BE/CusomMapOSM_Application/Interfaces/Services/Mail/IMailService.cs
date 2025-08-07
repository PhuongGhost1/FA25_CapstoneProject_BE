using CusomMapOSM_Application.Models.DTOs.Services;

namespace CusomMapOSM_Application.Interfaces.Services.Mail;

public interface IMailService
{
    Task SendEmailAsync(MailRequest mailRequest);
}
