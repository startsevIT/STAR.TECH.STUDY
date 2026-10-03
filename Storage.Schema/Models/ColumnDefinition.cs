namespace Storage.Schema.Models;

public sealed class ColumnDefinition
{
    public required string ColumnName { get; init; }
    public required string SqlType { get; init; }
    public bool IsNullable { get; init; }
    public string? DefaultValue { get; init; }
    public int? MaxLength { get; init; }

    public string GetColumnSql()
    {
        var sql = $"\"{ColumnName}\" {SqlType}";

        if (!IsNullable)
            sql += " NOT NULL";

        if (DefaultValue is not null)
            sql += $" DEFAULT {DefaultValue}";

        return sql;
    }
}