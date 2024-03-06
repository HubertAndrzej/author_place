using AuthorPlace.Models.Options;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class MailKitEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<SmtpOptions> smtpOptionsMonitor;

    public MailKitEmailSender(IOptionsMonitor<SmtpOptions> smtpOptionsMonitor)
    {
        this.smtpOptionsMonitor = smtpOptionsMonitor;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        SmtpOptions options = this.smtpOptionsMonitor.CurrentValue;
        using SmtpClient client = new();
        await client.ConnectAsync(options.Host, options.Port, options.Security);
        if (!string.IsNullOrEmpty(options.Username))
        {
            await client.AuthenticateAsync(options.Username, options.Password);
        }
        MimeMessage message = new();
        message.From.Add(MailboxAddress.Parse(options.Sender));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = htmlMessage
        };
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
