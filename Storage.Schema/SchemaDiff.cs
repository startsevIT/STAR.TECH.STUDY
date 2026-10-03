namespace Storage.Schema;

public sealed class SchemaDiff
{
    public IReadOnlyList<string> TableStatements { get; init; } = [];
    public IReadOnlyList<string> IndexStatements { get; init; } = [];
    public IReadOnlyList<string> ForeignKeyStatements { get; init; } = [];

    public bool IsEmpty =>
        TableStatements.Count == 0 &&
        IndexStatements.Count == 0 &&
        ForeignKeyStatements.Count == 0;

    public IReadOnlyList<string> AllStatements =>
        TableStatements
            .Concat(IndexStatements)
            .Concat(ForeignKeyStatements)
            .ToList();
}