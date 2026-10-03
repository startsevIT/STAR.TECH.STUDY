using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceCollectorDI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, Assembly serviceAssembly)
    {
        var serviceTypes = serviceAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.Name.EndsWith("Service", StringComparison.Ordinal));

        foreach (var implementationType in serviceTypes)
        {
            var interfaces = implementationType
                .GetInterfaces()
                .Where(i => i.Name.EndsWith("Service", StringComparison.Ordinal))
                .ToArray();

            foreach (var interfaceType in interfaces)
                services.AddScoped(interfaceType, implementationType);

            if (interfaces.Length == 0)
                services.AddScoped(implementationType);
        }

        return services;
    }
}