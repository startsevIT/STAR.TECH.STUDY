using Npgsql;
using Storage.Schema.Postgres;

namespace Storage.Schema;

public sealed class SchemaUpdater
{
    private readonly NpgsqlConnection _connection;
    private readonly PostgresSchemaExplorer _explorer;
    private readonly EntitySchemaProvider _modelProvider;
    private readonly string _schemaName;
    private readonly Action<string> _log;

    public SchemaUpdater(
        NpgsqlConnection connection,
        EntitySchemaProvider modelProvider,
        string schemaName,
        Action<string>? log = null)
    {
        _connection = connection;
        _explorer = new PostgresSchemaExplorer(connection, schemaName);
        _modelProvider = modelProvider;
        _schemaName = schemaName;
        _log = log ?? Console.WriteLine;
    }

    public SchemaDiff Preview()
    {
        EnsureSchemaExists();

        var model = _modelProvider.GetTables();
        var snapshot = _explorer.FetchSnapshot();
        var differ = new SchemaDiffer(_schemaName);

        return differ.Diff(model, snapshot);
    }

    public void EnhanceSchema(bool test)
    {
        EnsureSchemaExists();

        var diff = Preview();
        var statements = diff.AllStatements;

        if (statements.Count == 0)
        {
            _log("Schema is up to date.");
            return;
        }

        _log("Going to execute the following:\n" + string.Join("\n", statements));

        if (test)
            return;

        using var transaction = _connection.BeginTransaction();
        try
        {
            foreach (var statement in statements)
            {
                using var command = new NpgsqlCommand(statement, _connection, transaction);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private void EnsureSchemaExists()
    {
        if (!_explorer.SchemaExists())
            throw new InvalidOperationException(
                $"Schema '{_schemaName}' does not exist. " +
                $"Create it before running migrations.");
    }
}