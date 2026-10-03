using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace TokenCreator.JWT;

public sealed class TokenServiceOptions
{
    public const string SectionName = "Jwt";

    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; set; } = 60;

    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
}