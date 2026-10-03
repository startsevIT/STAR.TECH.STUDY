namespace Storage.Schema.Models;

public sealed class TableDefinition
{
    public required string TableName { get; init; }
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    public required IReadOnlyList<IndexDefinition> Indexes { get; init; }
    public required IReadOnlyList<ForeignKeyDefinition> ForeignKeys { get; init; }
    public required IReadOnlyList<string> PrimaryKeyColumns { get; init; }

    public string GetCreationSql(string schemaName)
    {
        var columns = string.Join(",\n    ",
            Columns.Select(c => c.GetColumnSql()));

        var pk = PrimaryKeyColumns.Count > 0
            ? $",\n    CONSTRAINT \"PK_{TableName}\" PRIMARY KEY ({string.Join(", ", PrimaryKeyColumns.Select(Quote))})"
            : string.Empty;

        return $"""
                CREATE TABLE {Quote(schemaName)}.{Quote(TableName)} (
                    {columns}{pk}
                );
                """;
    }

    public string? GetAlterTableAddColumnsSql(
        string schemaName,
        IReadOnlyCollection<string> existingColumns)
    {
        var newColumns = Columns
            .Where(c => !existingColumns.Contains(c.ColumnName))
            .ToList();

        if (newColumns.Count == 0)
            return null;

        var additions = string.Join(",\n    ",
            newColumns.Select(c => $"ADD COLUMN {c.GetColumnSql()}"));

        return $"""
                ALTER TABLE {Quote(schemaName)}.{Quote(TableName)}
                    {additions};
                """;
    }

    public bool HasNewColumnWithDefault(IReadOnlyCollection<string> existingColumns)
        => Columns.Any(c =>
            !existingColumns.Contains(c.ColumnName) &&
            c.DefaultValue is not null);

    private static string Quote(string identifier)
        => $"\"{identifier}\"";
}