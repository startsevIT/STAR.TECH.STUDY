using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordHasher.Abstractions;

namespace PasswordHasher.PBKDF2;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPasswordHasher(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services
                .AddOptions<PasswordHasherOptions>()
                .Bind(configuration.GetSection(PasswordHasherOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            return services;
        }

        public IServiceCollection AddPasswordHasher(Action<PasswordHasherOptions> configure)
        {
            services
                .AddOptions<PasswordHasherOptions>()
                .Configure(configure)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}