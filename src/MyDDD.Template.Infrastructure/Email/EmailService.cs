using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MyDDD.Template.Application.Abstractions;

namespace MyDDD.Template.Infrastructure.Email;

public sealed class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtpHost = configuration["MailHog:Smtp:Host"] ?? "localhost";
        var smtpPort = int.Parse(configuration["MailHog:Smtp:Port"] ?? "1025");

        using var client = new SmtpClient(smtpHost, smtpPort);
        var mailMessage = new MailMessage
        {
            From = new MailAddress("no-reply@myddd-template.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
