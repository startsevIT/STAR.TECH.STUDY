namespace Storage.Schema.Models;

public sealed class IndexDefinition
{
    public required string Name { get; init; }
    public required string TableName { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public bool IsUnique { get; init; }
    public string? Filter { get; init; }

    public string GetCreationSql(string schemaName)
    {
        var unique = IsUnique ? "UNIQUE " : string.Empty;
        var columns = string.Join(", ", Columns.Select(c => $"\"{c}\""));

        var filter = string.IsNullOrWhiteSpace(Filter)
            ? string.Empty
            : $"\n    WHERE {Filter}";

        return $"""
                CREATE {unique}INDEX "{Name}"
                    ON "{schemaName}"."{TableName}" ({columns}){filter};
                """;
    }
}