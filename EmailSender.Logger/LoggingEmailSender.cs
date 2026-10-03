using EmailSender.Abstractions;
using Microsoft.Extensions.Logging;

namespace EmailSender.Logger;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Email to {To}\nSubject: {Subject}\nBody: {Body}",
            to, subject, body);

        return Task.CompletedTask;
    }
}