using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TokenCreator.Abstractions;

namespace TokenCreator.JWT;

public sealed class TokenService : ITokenService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly TokenServiceOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(IOptions<TokenServiceOptions> options)
    {
        _options = options.Value;

        ValidateOptions(_options);

        var secretBytes = Encoding.UTF8.GetBytes(_options.Secret);
        var securityKey = new SymmetricSecurityKey(secretBytes);
        _signingCredentials = new SigningCredentials(securityKey, _options.Algorithm);
    }

    public string CreateToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = _signingCredentials
        };

        var token = TokenHandler.CreateToken(descriptor);
        return TokenHandler.WriteToken(token);
    }

    private static void ValidateOptions(TokenServiceOptions options)
    {
        var minSecretLength = options.Algorithm switch
        {
            SecurityAlgorithms.HmacSha256 => 32,
            SecurityAlgorithms.HmacSha384 => 48,
            SecurityAlgorithms.HmacSha512 => 64,
            _ => throw new ArgumentException(
                $"Unsupported algorithm: {options.Algorithm}. " +
                $"Supported: {SecurityAlgorithms.HmacSha256}, " +
                $"{SecurityAlgorithms.HmacSha384}, {SecurityAlgorithms.HmacSha512}.",
                nameof(options.Algorithm))
        };

        var secretLength = Encoding.UTF8.GetByteCount(options.Secret);
        if (secretLength < minSecretLength)
            throw new ArgumentException(
                $"Secret must be at least {minSecretLength} bytes for {options.Algorithm}.",
                nameof(options.Secret));
    }
}