using EmailSender.Abstractions;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailSender.SMTP;

public sealed class SmtpEmailSender(IOptions<SmtpEmailSenderOptions> options) : IEmailSender
{
    private readonly SmtpEmailSenderOptions _options = options.Value;

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
            ct);

        await client.AuthenticateAsync(_options.Username, _options.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}