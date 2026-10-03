using Domain.Users;
using EmailSender.Abstractions;
using Microsoft.Extensions.Options;
using Storage.Abstractions;

namespace Application.Users;

public class EmailConfirmationService(
    IStorage storage,
    IEmailSender emailSender,
    IOptions<AppOptions> options)
{
    private static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromMinutes(15);

    public void Confirm(string token)
    {
        var confirmationToken = storage.SingleOrThrow<EmailConfirmationToken>(x => x.Token == token);
        if (confirmationToken.IsExpired)
            throw new InvalidOperationException("Confirmation token expired.");

        var credential = GetUnconfirmedCredential(confirmationToken.UserId);

        storage.Transaction(() =>
        {
            credential.ConfirmEmail();
            storage.Update(credential);
            storage.Delete<EmailConfirmationToken>(confirmationToken.Id);
        });
    }

    public async Task ResendAsync(Guid userId)
    {
        var email = GetUnconfirmedCredential(userId).Email;
        
        var token = EmailConfirmationToken.Create(userId, ConfirmationTokenLifetime);
        storage.Transaction(() =>
        {
            storage.DeleteWhere<EmailConfirmationToken>(x => x.UserId == userId);
            storage.Insert(token);
        });

        var body =
            $"""
             Click to confirm: 
             <a href=\"http://{options.Value.Host}/api/email_confirmation/confirm-email?token={Uri.EscapeDataString(token.Token)}\">
                 Подтвердить
             </a>
             """;
        
        await emailSender.SendAsync(email, "Email confirmation", body);
    }

    private UserCredential GetUnconfirmedCredential(Guid userId)
    {
        var credential = storage.SingleOrThrow<UserCredential>(x => x.UserId == userId);
        return credential.IsEmailConfirmed
            ? throw new InvalidOperationException("Email is already confirmed.")
            : credential;
    }
}