using System.Reflection;
using Storage.Abstractions;

namespace Storage.Reflection;

public static class EntityTypeScanner
{
    public static IReadOnlyList<Type> GetEntities(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(IEntity).IsAssignableFrom(t))
            .ToList();
    }
}