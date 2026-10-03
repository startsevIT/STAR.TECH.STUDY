using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storage.Abstractions;

namespace Storage.Validators;

public static class EntityConfigurationValidator
{
    public static void Validate(Assembly domainAssembly, Assembly infrastructureAssembly)
    {
        var entityTypes = domainAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(IEntity).IsAssignableFrom(t))
            .ToList();

        var configuredTypes = infrastructureAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var missing = entityTypes
            .Where(e => !configuredTypes.Contains(e))
            .ToList();

        if (missing.Count > 0)
        {
            var names = string.Join(", ", missing.Select(t => t.FullName));
            throw new InvalidOperationException(
                $"Не найдены IEntityTypeConfiguration для сущностей: {names}");
        }
    }
}