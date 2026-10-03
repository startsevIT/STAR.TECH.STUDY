using System.Security.Cryptography;
using Domain.Abstractions;

namespace Domain.Users;

public sealed class EmailConfirmationToken : Entity
{
    private EmailConfirmationToken(
        Guid userId,
        string token,
        DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private init; }
    public string Token { get; private init; }
    public DateTime ExpiresAt { get; private init; }

    public static EmailConfirmationToken Create(Guid userId, TimeSpan lifetime)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return new EmailConfirmationToken(userId, token, DateTime.UtcNow.Add(lifetime));
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}

public class EmailNotConfirmedException : Exception;
