using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FromToken;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTokenUser(
        this IServiceCollection services,
        Action<TokenUserOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<TokenUserOptions>();

        services.Configure<TokenUserOptions>(options =>
        {
            options.AddParser<string>(value => value);
            options.AddParser(value => Guid.TryParse(value, out var id) ? id : Guid.Empty);
            options.AddParser(value => long.TryParse(value, out var id) ? id : 0);
            options.AddParser(value => int.TryParse(value, out var id) ? id : 0);

            options.AddClaimAlias(
                "userId",
                "sub",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        });

        if (configure is not null)
            services.Configure(configure);

        services.Configure<MvcOptions>(options =>
        {
            options.ModelBinderProviders.Insert(0, new UserIdModelBinderProvider());
        });

        services.Configure<SwaggerGenOptions>(options =>
        {
            options.OperationFilter<FromTokenOperationFilter>();
        });

        return services;
    }

    public static TokenUserOptions AddJsonParser<T>(this TokenUserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddParser<T>(value =>
            JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }));

        return options;
    }
}