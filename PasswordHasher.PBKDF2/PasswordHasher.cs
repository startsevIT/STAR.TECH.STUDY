using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PasswordHasher.Abstractions;

namespace PasswordHasher.PBKDF2;

public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly HashSet<string> SupportedAlgorithms = new(StringComparer.Ordinal)
    {
        "SHA1", "SHA256", "SHA384", "SHA512"
    };

    private Lazy<(string Hash, string Salt)> DummyCredentials =>
        new(() => Hash("dummy-password-for-timing-protection"));

    private readonly PasswordHasherOptions _options;
    private readonly HashAlgorithmName _hashAlgorithm;

    public PasswordHasher(IOptions<PasswordHasherOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;

        ValidateOptions(_options);

        _hashAlgorithm = new HashAlgorithmName(_options.Algorithm);
    }

    public (string Hash, string Salt) Hash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(_options.SaltSize);
        var hashBytes = ComputeHash(password, saltBytes);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(password)
            || string.IsNullOrEmpty(hash)
            || string.IsNullOrEmpty(salt))
            return false;

        byte[] hashBytes, saltBytes;
        try
        {
            hashBytes = Convert.FromBase64String(hash);
            saltBytes = Convert.FromBase64String(salt);
        }
        catch (FormatException)
        {
            return false;
        }

        if (saltBytes.Length != _options.SaltSize)
            return false;

        var computedHash = ComputeHash(password, saltBytes);
        return CryptographicOperations.FixedTimeEquals(hashBytes, computedHash);
    }

    public bool FakeVerify(string password)
    {
        var (hash, salt) = DummyCredentials.Value;
        return Verify(password, hash, salt);
    }

    private byte[] ComputeHash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _options.IterationCount,
            _hashAlgorithm,
            _options.HashSize);

    private static void ValidateOptions(PasswordHasherOptions options)
    {
        if (!SupportedAlgorithms.Contains(options.Algorithm))
            throw new ArgumentException(
                $"Unsupported hash algorithm: {options.Algorithm}. " +
                $"Supported: {string.Join(", ", SupportedAlgorithms)}.",
                nameof(options.Algorithm));
    }
}