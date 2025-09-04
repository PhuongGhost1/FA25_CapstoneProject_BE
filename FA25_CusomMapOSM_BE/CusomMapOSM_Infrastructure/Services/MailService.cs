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
        mimeMessage.From.Add(new MailboxAddress("Custom Map OSM Organization", _smtpUsername));
        mimeMessage.To.Add(new MailboxAddress("Receiver Name", mailRequest.ToEmail));
        mimeMessage.Subject = mailRequest.Subject;

        // Enhanced HTML template with modern design
        string htmlBody = $@"
        <!DOCTYPE html>
        <html xmlns=""http://www.w3.org/1999/xhtml"">
        <head>
            <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
            <title>{mailRequest.Subject}</title>
            <style>
                /* Base styles */
                body {{
                    margin: 0;
                    padding: 0;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: #333333;
                    background-color: #f7f9fc;
                }}
                .container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background: #ffffff;
                    border-radius: 8px;
                    overflow: hidden;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.05);
                }}
                .header {{
                    background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
                    padding: 30px 20px;
                    text-align: center;
                }}
                .logo {{
                    color: white;
                    font-size: 28px;
                    font-weight: bold;
                    text-decoration: none;
                }}
                .content {{
                    padding: 30px;
                }}
                .footer {{
                    background: #f1f5f9;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                    color: #64748b;
                }}
                .social-links a {{
                    margin: 0 10px;
                    text-decoration: none;
                }}
                .social-icon {{
                    width: 24px;
                    height: 24px;
                    vertical-align: middle;
                }}
                .button {{
                    display: inline-block;
                    padding: 12px 30px;
                    margin: 20px 0;
                    background: #3b82f6;
                    color: white !important;
                    text-decoration: none;
                    border-radius: 4px;
                    font-weight: 600;
                }}
                /* Responsive adjustments */
                @media only screen and (max-width: 600px) {{
                    .container {{
                        margin: 10px;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <div class=""header"">
                    <a href=""https://yourdomain.com"" class=""logo"">
                        Custom Map OSM
                    </a>
                </div>
                
                <div class=""content"">
                    <!-- Dynamic Content Injection -->
                    {mailRequest.Body}
                    
                    <!-- Optional CTA Button -->
                    <a href=""https://yourdomain.com/dashboard"" class=""button"">
                        Go to Dashboard
                    </a>
                </div>
                
                <div class=""footer"">
                    <p>© {DateTime.Now.Year} Custom Map OSM Organization. All rights reserved.</p>
                    <p>123 Map Street, GeoCity | contact@custommap.org</p>
                    
                    <div class=""social-links"">
                        <a href=""https://twitter.com/yourhandle"">
                            <img src=""https://i.imgur.com/xS6y5YX.png"" alt=""Twitter"" class=""social-icon"">
                        </a>
                        <a href=""https://linkedin.com/company/yourcompany"">
                            <img src=""https://i.imgur.com/Q4dEZXI.png"" alt=""LinkedIn"" class=""social-icon"">
                        </a>
                        <a href=""https://github.com/yourrepo"">
                            <img src=""https://i.imgur.com/4W1qsKb.png"" alt=""GitHub"" class=""social-icon"">
                        </a>
                    </div>
                    
                    <p style=""margin-top: 15px; font-size: 11px; color: #94a3b8;"">
                        You're receiving this email because you have an account with Custom Map OSM.
                        <br><a href=""#"" style=""color: #64748b;"">Unsubscribe</a> | <a href=""#"" style=""color: #64748b;"">Preferences</a>
                    </p>
                </div>
            </div>
        </body>
        </html>";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            // Add text alternative for non-HTML clients
            TextBody = $"Custom Map OSM Notification\n\n{StripHtml(mailRequest.Body)}\n\n"
                    + $"Go to Dashboard: https://yourdomain.com/dashboard\n"
                    + $"\n© {DateTime.Now.Year} Custom Map OSM Organization"
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
        client.Authenticate(_smtpUsername, _smtpPassword);
        client.Send(mimeMessage);
        client.Disconnect(true);

        return Task.CompletedTask;
    }

    // Helper method to create plain text alternative
    private string StripHtml(string html)
    {
        return Regex.Replace(html, "<[^>]*>", "");
    }
}