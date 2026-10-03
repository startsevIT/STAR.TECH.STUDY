using EmailSender.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSender.Logger;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingEmailSender(this IServiceCollection services)
    {
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        return services;
    }
}