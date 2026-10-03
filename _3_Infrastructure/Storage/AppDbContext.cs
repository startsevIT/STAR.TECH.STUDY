using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using SnakeCaseConverter;
using Storage.Reflection;

namespace Infrastructure.Storage;

public class AppDbContext(DbContextOptions builder) : DbContext(builder)
{
    public DbSet<T> GetDbSet<T>()
        where T : Entity => Set<T>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Entity).Assembly);
        foreach (var type in EntityTypeScanner.GetEntities(typeof(Entity).Assembly))
            modelBuilder.Entity(type);

        var entityClrTypes = modelBuilder.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .Distinct()
            .ToArray();

        foreach (var clrType in entityClrTypes)
        {
            var entityBuilder = modelBuilder.Entity(clrType);
            var properties = clrType.GetProperties()
                .Where(p => (Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType).IsEnum);

            foreach (var prop in properties)
                entityBuilder.Property(prop.Name).HasConversion<string>();
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetTableName(SnakeCaseNamingConverter.ToSnakeCase(entityType.GetTableName()));

            foreach (var property in entityType.GetProperties())
                property.SetColumnName(SnakeCaseNamingConverter.ToSnakeCase(property.GetColumnName()));
        }
    }
}