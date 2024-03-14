using Microsoft.AspNetCore.Identity.UI.Services;

namespace AuthorPlace.Models.Services.Infrastructure.Interfaces;

public interface IEmailClient : IEmailSender
{
    Task SendEmailAsync(string recipientEmail, string? replyToEmail, string subject, string htmlMessage, CancellationToken token = default);
}
