namespace Storage.Schema.Models;

public sealed class ForeignKeyDefinition
{
    public required string Name { get; init; }
    public required string TableName { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public required string PrincipalTable { get; init; }
    public required IReadOnlyList<string> PrincipalColumns { get; init; }
    public string OnDelete { get; init; } = "RESTRICT";

    public string GetCreationSql(string schemaName)
    {
        var cols = string.Join(", ", Columns.Select(c => $"\"{c}\""));
        var principalCols = string.Join(", ", PrincipalColumns.Select(c => $"\"{c}\""));

        return $"""
                ALTER TABLE "{schemaName}"."{TableName}"
                    ADD CONSTRAINT "{Name}"
                    FOREIGN KEY ({cols})
                    REFERENCES "{schemaName}"."{PrincipalTable}" ({principalCols})
                    ON DELETE {OnDelete};
                """;
    }
}