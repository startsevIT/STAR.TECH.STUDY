using Domain.Abstractions;
using PasswordHasher.Abstractions;

namespace Domain.Users;

public sealed class UserCredential : Entity
{
    private UserCredential(
        Guid userId,
        string email,
        string phone,
        string passwordHash,
        string passwordSalt)
    {
        UserId = userId;
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        IsEmailConfirmed = false;
        IsPhoneConfirmed = false;
    }

    public Guid UserId { get; private init; }
    public string Email { get; private init; }
    public string Phone { get; private init; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }
    public string PasswordHash { get; private init; }
    public string PasswordSalt { get; private init; }

    public static UserCredential Create(Guid userId, string email, string phone, string hash, string salt) =>
        new(userId, email, phone, hash, salt);

    public bool Verify(string? password, IPasswordHasher hasher) =>
        password is not null && hasher.Verify(password, PasswordHash, PasswordSalt);

    public void ConfirmEmail()
    {
        if (IsEmailConfirmed)
            throw new InvalidOperationException("Email is already confirmed.");

        IsEmailConfirmed = true;
    }
}

public class InvalidCredentialsException : Exception;