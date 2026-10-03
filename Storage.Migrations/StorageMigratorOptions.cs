namespace Storage.Migrations;

public sealed class StorageMigratorOptions
{
    public const string SectionName = "StorageMigrator";

    public string SchemaName { get; set; } = "public";

    public bool IgnoreStubs { get; set; }

    public int UnsafeIndexRowsCountLimit { get; set; } = 10_000;

    public int UnsafeTablesRowsCountLimit { get; set; } = 10_000;

    public string? EntitiesAssemblyName { get; set; }
}