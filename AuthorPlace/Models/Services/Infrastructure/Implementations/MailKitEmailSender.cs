using AuthorPlace.Models.Options;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class MailKitEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<SmtpOptions> smtpOptionsMonitor;
    private readonly IConfiguration configuration;

    public MailKitEmailSender(IOptionsMonitor<SmtpOptions> smtpOptionsMonitor, IConfiguration configuration)
    {
        this.smtpOptionsMonitor = smtpOptionsMonitor;
        this.configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
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
