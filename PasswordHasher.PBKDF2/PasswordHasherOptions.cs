using System.ComponentModel.DataAnnotations;

namespace PasswordHasher.PBKDF2;

public sealed class PasswordHasherOptions
{
    public const string SectionName = "PasswordHasher";

    [Range(8, 128)] public int SaltSize { get; set; } = 16;

    [Range(16, 128)] public int HashSize { get; set; } = 32;

    [Range(10_000, int.MaxValue)] public int IterationCount { get; set; } = 600_000;

    [Required] public string Algorithm { get; set; } = "SHA256";
}