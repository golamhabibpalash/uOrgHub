using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace uOrgHub.Auth.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendAsync(string to, string subject, string body)
    {
        var fromName = _config["EmailSettings:FromName"] ?? "uOrgHub ERP";
        var fromEmail = _config["EmailSettings:FromEmail"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["EmailSettings:SmtpHost"]!,
            int.Parse(_config["EmailSettings:SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _config["EmailSettings:SmtpUser"]!,
            _config["EmailSettings:SmtpPassword"]!);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendOtpAsync(string to, string code, string purpose)
    {
        var subject = purpose switch
        {
            "Login" => "uOrgHub – Two-Factor Authentication Code",
            "PasswordReset" => "uOrgHub – Password Reset Code",
            "EmailVerify" => "uOrgHub – Email Verification Code",
            _ => "uOrgHub – Verification Code",
        };

        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:480px;margin:0 auto;">
              <h2 style="color:#4f46e5;">uOrgHub ERP</h2>
              <p>Your verification code is:</p>
              <div style="font-size:32px;font-weight:bold;letter-spacing:8px;color:#1e293b;padding:16px 0;">{code}</div>
              <p style="color:#64748b;">This code expires in 10 minutes. Do not share it with anyone.</p>
            </div>
            """;

        await SendAsync(to, subject, body);
    }
}
