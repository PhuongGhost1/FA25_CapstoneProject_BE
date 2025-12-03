using System.Text.RegularExpressions;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Common.ServiceConstants;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CusomMapOSM_Infrastructure.Services;

public class MailService : IMailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public MailService()
    {
        _smtpServer = MailKitConstant.SmtpServer;
        _smtpPort = MailKitConstant.SmtpPort;
        _smtpUsername = MailKitConstant.SmtpUsername;
        _smtpPassword = MailKitConstant.SmtpPassword;
    }

    public Task SendEmailAsync(MailRequest mailRequest)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("IMOS", _smtpUsername));
        mimeMessage.To.Add(new MailboxAddress("Receiver Name", mailRequest.ToEmail));
        mimeMessage.Subject = mailRequest.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = mailRequest.Body,
            TextBody = $"IMOS Notification\n\n{StripHtml(mailRequest.Body)}\n\n"
                    + $"Go to Dashboard: https://yourdomain.com/dashboard\n"
                    + $"\n© {DateTime.Now.Year} IMOS. All rights reserved."
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
        client.Authenticate(_smtpUsername, _smtpPassword);
        client.Send(mimeMessage);
        client.Disconnect(true);

        return Task.CompletedTask;
    }

    private string StripHtml(string html)
    {
        return Regex.Replace(html, "<[^>]*>", "");
    }
}