namespace Storage.Schema;

public sealed class SchemaSnapshot
{
    public required IReadOnlyDictionary<string, TableSnapshot> Tables { get; init; }
}

public sealed class TableSnapshot
{
    public required IReadOnlyDictionary<string, string> Columns { get; init; }
    public required IReadOnlyList<IndexSnapshot> Indexes { get; init; }
    public required IReadOnlyList<ForeignKeySnapshot> ForeignKeys { get; init; }
}

public sealed class IndexSnapshot
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public bool IsUnique { get; init; }
    public string? Filter { get; init; }
}

public sealed class ForeignKeySnapshot
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public required string PrincipalTable { get; init; }
    public required IReadOnlyList<string> PrincipalColumns { get; init; }
}