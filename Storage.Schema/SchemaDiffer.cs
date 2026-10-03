using Storage.Schema.Models;

namespace Storage.Schema;

public sealed class SchemaDiffer
{
    private readonly string _schemaName;

    public SchemaDiffer(string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        _schemaName = schemaName;
    }

    public SchemaDiff Diff(
        IReadOnlyList<TableDefinition> model,
        SchemaSnapshot current)
    {
        var tableStatements = new List<string>();
        var indexStatements = new List<string>();
        var fkStatements = new List<string>();

        foreach (var table in model)
        {
            if (current.Tables.TryGetValue(table.TableName, out var currentTable))
            {
                EnsureColumnsCompatible(table, currentTable.Columns);

                var alter = table.GetAlterTableAddColumnsSql(
                    _schemaName,
                    currentTable.Columns.Keys.ToHashSet());

                if (alter is not null)
                    tableStatements.Add(alter);

                var currentIndexNames = currentTable.Indexes
                    .Select(i => i.Name)
                    .ToHashSet();

                foreach (var index in table.Indexes.Where(i => !currentIndexNames.Contains(i.Name)))
                    indexStatements.Add(index.GetCreationSql(_schemaName));

                var currentFkNames = currentTable.ForeignKeys
                    .Select(fk => fk.Name)
                    .ToHashSet();

                foreach (var fk in table.ForeignKeys.Where(fk => !currentFkNames.Contains(fk.Name)))
                    fkStatements.Add(fk.GetCreationSql(_schemaName));
            }
            else
            {
                tableStatements.Add(table.GetCreationSql(_schemaName));

                foreach (var index in table.Indexes)
                    indexStatements.Add(index.GetCreationSql(_schemaName));

                foreach (var fk in table.ForeignKeys)
                    fkStatements.Add(fk.GetCreationSql(_schemaName));
            }
        }

        return new SchemaDiff
        {
            TableStatements = tableStatements,
            IndexStatements = indexStatements,
            ForeignKeyStatements = fkStatements
        };
    }

    private void EnsureColumnsCompatible(
        TableDefinition table,
        IReadOnlyDictionary<string, string> currentColumns)
    {
        foreach (var column in table.Columns)
        {
            if (!currentColumns.TryGetValue(column.ColumnName, out var currentType))
                continue;

            if (!AreTypesCompatible(column.SqlType, currentType))
            {
                throw new InvalidOperationException(
                    $"Column '{table.TableName}.{column.ColumnName}' has incompatible type. " +
                    $"Model: '{column.SqlType}', database: '{currentType}'. " +
                    $"Type changes are not supported by the schema updater.");
            }
        }
    }

    private static bool AreTypesCompatible(string modelType, string dbType)
        => NormalizeType(modelType) == NormalizeType(dbType);

    private static string NormalizeType(string type)
    {
        var lower = type.Trim().ToLowerInvariant();

        var parenIndex = lower.IndexOf('(');
        if (parenIndex >= 0)
            lower = lower[..parenIndex].Trim();

        return lower switch
        {
            "text" or "varchar" or "character varying" or "char" or "character" => "text",
            "uuid" => "uuid",
            "integer" or "int" or "int4" => "integer",
            "bigint" or "int8" => "bigint",
            "smallint" or "int2" => "smallint",
            "boolean" or "bool" => "boolean",
            "numeric" or "decimal" => "numeric",
            "real" or "float4" => "real",
            "double precision" or "float8" or "double" => "double precision",
            "timestamp with time zone" or "timestamptz" => "timestamptz",
            "timestamp without time zone" or "timestamp" => "timestamp",
            "date" => "date",
            "time" or "time without time zone" => "time",
            "time with time zone" or "timetz" => "timetz",
            "json" => "json",
            "jsonb" => "jsonb",
            "bytea" => "bytea",
            _ => lower
        };
    }
}