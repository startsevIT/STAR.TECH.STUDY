using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SnakeCaseConverter;
using Storage.Schema.Models;

namespace Storage.Schema;

public sealed class EntitySchemaProvider(IModel model)
{
    public IReadOnlyList<TableDefinition> GetTables() =>
        model.GetEntityTypes()
            .Where(t => !t.IsOwned())
            .Where(t => t.GetTableName() is not null)
            .Select(BuildTableDefinition)
            .ToList();

    private static TableDefinition BuildTableDefinition(IEntityType entityType)
    {
        var tableName = SnakeCaseNamingConverter.ToSnakeCase(entityType.GetTableName()!);

        var columns = entityType.GetProperties()
            .Where(p => !p.IsShadowProperty() || p.IsForeignKey())
            .Select(BuildColumnDefinition)
            .ToList();

        var primaryKey = entityType.FindPrimaryKey()?
            .Properties
            .Select(p => SnakeCaseNamingConverter.ToSnakeCase(p.GetColumnName()))
            .ToArray() ?? [];

        var indexes = entityType.GetIndexes()
            .Where(i => !IsImplicitPrimaryKeyIndex(i, entityType))
            .Select(i => BuildIndexDefinition(i, tableName))
            .ToList();

        var foreignKeys = entityType.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.GetTableName() is not null)
            .Select(fk => BuildForeignKeyDefinition(fk, tableName))
            .ToList();

        return new TableDefinition
        {
            TableName = tableName,
            Columns = columns,
            PrimaryKeyColumns = primaryKey,
            Indexes = indexes,
            ForeignKeys = foreignKeys
        };
    }

    private static ColumnDefinition BuildColumnDefinition(IProperty property)
    {
        var columnName = SnakeCaseNamingConverter.ToSnakeCase(property.GetColumnName());

        return new ColumnDefinition
        {
            ColumnName = columnName,
            SqlType = MapToPostgres(property),
            IsNullable = property.IsNullable,
            MaxLength = property.GetMaxLength(),
            DefaultValue = property.GetDefaultValueSql()
        };
    }

    private static string MapToPostgres(IProperty property)
    {
        var explicitType = property.GetColumnType();
        if (!string.IsNullOrEmpty(explicitType) && explicitType != "text")
            return explicitType;

        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (clrType == typeof(Guid))              return "uuid";
        if (clrType == typeof(int))               return "integer";
        if (clrType == typeof(long))              return "bigint";
        if (clrType == typeof(short))             return "smallint";
        if (clrType == typeof(bool))              return "boolean";
        if (clrType == typeof(decimal))           return "numeric(18,2)";
        if (clrType == typeof(double))            return "double precision";
        if (clrType == typeof(float))             return "real";
        if (clrType == typeof(DateTime))          return "timestamp with time zone";
        if (clrType == typeof(DateTimeOffset))    return "timestamp with time zone";
        if (clrType == typeof(DateOnly))          return "date";
        if (clrType == typeof(TimeOnly))          return "time";
        if (clrType == typeof(string))            return "text";
        if (clrType.IsEnum)                       return "text";

        throw new NotSupportedException($"Тип {clrType.Name} не поддерживается.");
    }

    private static IndexDefinition BuildIndexDefinition(IIndex index, string tableName)
    {
        var columns = index.Properties
            .Select(p => SnakeCaseNamingConverter.ToSnakeCase(p.GetColumnName()))
            .ToArray();

        var name = index.GetDatabaseName()
            ?? $"ix_{tableName}_{string.Join("_", columns)}";

        return new IndexDefinition
        {
            Name = SnakeCaseNamingConverter.ToSnakeCase(name),
            TableName = tableName,
            Columns = columns,
            IsUnique = index.IsUnique,
            Filter = index.GetFilter()
        };
    }

    private static ForeignKeyDefinition BuildForeignKeyDefinition(IForeignKey fk, string tableName)
    {
        var columns = fk.Properties
            .Select(p => SnakeCaseNamingConverter.ToSnakeCase(p.GetColumnName()))
            .ToArray();

        var principalTable = SnakeCaseNamingConverter.ToSnakeCase(
            fk.PrincipalEntityType.GetTableName()!);

        var principalColumns = fk.PrincipalKey.Properties
            .Select(p => SnakeCaseNamingConverter.ToSnakeCase(p.GetColumnName()))
            .ToArray();

        var name = fk.GetConstraintName()
            ?? $"fk_{tableName}_{string.Join("_", columns)}";

        return new ForeignKeyDefinition
        {
            Name = SnakeCaseNamingConverter.ToSnakeCase(name),
            TableName = tableName,
            Columns = columns,
            PrincipalTable = principalTable,
            PrincipalColumns = principalColumns,
            OnDelete = fk.DeleteBehavior switch
            {
                DeleteBehavior.Cascade => "CASCADE",
                DeleteBehavior.Restrict => "RESTRICT",
                DeleteBehavior.SetNull => "SET NULL",
                DeleteBehavior.NoAction => "NO ACTION",
                _ => "RESTRICT"
            }
        };
    }

    private static bool IsImplicitPrimaryKeyIndex(IIndex index, IEntityType entityType)
    {
        var pk = entityType.FindPrimaryKey();
        if (pk is null) return false;

        return index.Properties.SequenceEqual(pk.Properties) && index.IsUnique;
    }
}