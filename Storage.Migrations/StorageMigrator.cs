using Microsoft.EntityFrameworkCore;
using Npgsql;
using Storage.Schema;

namespace Storage.Migrations;

public sealed class StorageMigrator
{
    private readonly NpgsqlConnection _connection;
    private readonly EntitySchemaProvider _modelProvider;
    private readonly StorageMigratorOptions _options;

    public StorageMigrator(
        NpgsqlConnection connection,
        DbContext context,
        StorageMigratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        _connection = connection;
        _modelProvider = new EntitySchemaProvider(context.Model);
        _options = options;
    }

    public IReadOnlyList<string> Check()
        => CreateUpdater().Preview().AllStatements;

    public void Migrate()
        => CreateUpdater().EnhanceSchema(test: false);

    public void EnsureUpToDate()
    {
        var preview = CreateUpdater().Preview().AllStatements;

        if (preview.Count > 0)
            throw new InvalidOperationException(
                "Schema is not up to date. Pending changes:\n" +
                string.Join("\n", preview));
    }

    private SchemaUpdater CreateUpdater()
        => new(_connection, _modelProvider, _options.SchemaName);
}