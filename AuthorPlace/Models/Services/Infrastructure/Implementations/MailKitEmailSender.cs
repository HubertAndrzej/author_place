using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class MailKitEmailSender : IEmailClient
{
    private readonly IOptionsMonitor<SmtpOptions> smtpOptionsMonitor;
    private readonly IConfiguration configuration;

    public MailKitEmailSender(IOptionsMonitor<SmtpOptions> smtpOptionsMonitor, IConfiguration configuration)
    {
        this.smtpOptionsMonitor = smtpOptionsMonitor;
        this.configuration = configuration;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return SendEmailAsync(email, string.Empty, subject, htmlMessage);
    }

    public async Task SendEmailAsync(string recipientEmail, string replyToEmail, string subject, string htmlMessage)
    {
        SmtpOptions options = this.smtpOptionsMonitor.CurrentValue;
        using SmtpClient client = new();
        await client.ConnectAsync(options.Host, options.Port, options.Security);
        if (!string.IsNullOrEmpty(configuration["Smtp:Username"]))
        {
            await client.AuthenticateAsync(configuration["Smtp:Username"], configuration["Smtp:Password"]);
        }
        MimeMessage message = new();
        message.From.Add(MailboxAddress.Parse(options.Sender));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        if (replyToEmail is not (null or ""))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(replyToEmail));
        }
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = htmlMessage
        };
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
