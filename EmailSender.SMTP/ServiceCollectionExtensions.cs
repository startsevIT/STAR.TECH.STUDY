using EmailSender.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSender.SMTP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmtpEmailSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SmtpEmailSenderOptions>()
            .Bind(configuration.GetSection(SmtpEmailSenderOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}