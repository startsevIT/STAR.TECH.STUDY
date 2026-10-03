using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storage.Abstractions;

namespace Storage.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfStorage<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> builder)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builder);

        services.AddDbContext<TContext>(builder);

        return services.AddEfStorageCore<TContext>();
    }

    public static IServiceCollection AddEfStorage<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddEfStorageCore<TContext>();
    }

    private static IServiceCollection AddEfStorageCore<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped<IStorage, DbStorage>();

        return services;
    }
}