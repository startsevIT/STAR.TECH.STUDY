using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TokenCreator.Abstractions;

namespace TokenCreator.JWT;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtTokenService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(TokenServiceOptions.SectionName);
        var options = section.Get<TokenServiceOptions>()
                      ?? throw new InvalidOperationException(
                          $"Configuration section '{TokenServiceOptions.SectionName}' is missing.");

        services
            .AddOptions<TokenServiceOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return AddJwtTokenServiceCore(services, options);
    }

    public static IServiceCollection AddJwtTokenService(
        this IServiceCollection services,
        Action<TokenServiceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new TokenServiceOptions();
        configure(options);

        services
            .AddOptions<TokenServiceOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return AddJwtTokenServiceCore(services, options);
    }

    private static IServiceCollection AddJwtTokenServiceCore(
        IServiceCollection services,
        TokenServiceOptions options)
    {
        services.AddSingleton<ITokenService, TokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,

                    ValidateAudience = true,
                    ValidAudience = options.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(options.Secret)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization(auth =>
        {
            auth.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}