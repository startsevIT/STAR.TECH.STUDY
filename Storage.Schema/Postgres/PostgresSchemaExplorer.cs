using Npgsql;

namespace Storage.Schema.Postgres;

public sealed class PostgresSchemaExplorer
{
    private readonly NpgsqlConnection _connection;
    private readonly string _schemaName;

    public PostgresSchemaExplorer(NpgsqlConnection connection, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        _connection = connection;
        _schemaName = schemaName;
    }

    public SchemaSnapshot FetchSnapshot()
    {
        var columnsByTable = ReadColumns();
        var indexesByTable = ReadIndexes();
        var fksByTable = ReadForeignKeys();

        var tableNames = columnsByTable.Keys
            .Union(indexesByTable.Keys, StringComparer.OrdinalIgnoreCase)
            .Union(fksByTable.Keys, StringComparer.OrdinalIgnoreCase);

        var tables = new Dictionary<string, TableSnapshot>(StringComparer.OrdinalIgnoreCase);

        foreach (var tableName in tableNames)
        {
            tables[tableName] = new TableSnapshot
            {
                Columns = columnsByTable.GetValueOrDefault(tableName)
                          ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Indexes = indexesByTable.GetValueOrDefault(tableName) ?? [],
                ForeignKeys = fksByTable.GetValueOrDefault(tableName) ?? []
            };
        }

        return new SchemaSnapshot { Tables = tables };
    }

    public bool SchemaExists()
    {
        const string sql = """
                           SELECT 1
                           FROM information_schema.schemata
                           WHERE schema_name = @schema;
                           """;

        using var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddWithValue("schema", _schemaName);

        return command.ExecuteScalar() is not null;
    }

    public string[] FetchTableNames()
    {
        const string sql = """
                           SELECT table_name
                           FROM information_schema.tables
                           WHERE table_schema = @schema
                             AND table_type = 'BASE TABLE';
                           """;

        var result = new List<string>();

        using var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddWithValue("schema", _schemaName);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(reader.GetString(0));

        return result.ToArray();
    }

    public string? GetColumnType(string tableName, string columnName)
    {
        const string sql = """
                           SELECT data_type
                           FROM information_schema.columns
                           WHERE table_schema = @schema
                             AND table_name = @table
                             AND column_name = @column;
                           """;

        using var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddWithValue("schema", _schemaName);
        command.Parameters.AddWithValue("table", tableName);
        command.Parameters.AddWithValue("column", columnName);

        return command.ExecuteScalar() as string;
    }

    public long GetRowsCount(string tableName)
    {
        const string sql = """
                           SELECT n_live_tup
                           FROM pg_stat_user_tables
                           WHERE schemaname = @schema AND relname = @table;
                           """;

        using var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddWithValue("schema", _schemaName);
        command.Parameters.AddWithValue("table", tableName);

        var result = command.ExecuteScalar();
        return result is long count ? count : 0;
    }

    private Dictionary<string, Dictionary<string, string>> ReadColumns()
    {
        const string sql = """
                           SELECT
                               t.relname AS table_name,
                               a.attname AS column_name,
                               format_type(a.atttypid, a.atttypmod) AS data_type
                           FROM pg_attribute a
                           JOIN pg_class t ON t.oid = a.attrelid
                           JOIN pg_namespace n ON n.oid = t.relnamespace
                           WHERE n.nspname = @schema
                             AND t.relkind = 'r'
                             AND a.attnum > 0
                             AND NOT a.attisdropped
                           ORDER BY t.relname, a.attnum;
                           """;

        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("schema", _schemaName);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            var type = reader.GetString(2);

            if (!result.TryGetValue(table, out var cols))
            {
                cols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                result[table] = cols;
            }

            cols[column] = type;
        }

        return result;
    }

    private Dictionary<string, List<IndexSnapshot>> ReadIndexes()
    {
        const string sql = """
                           SELECT
                               t.relname AS table_name,
                               i.relname AS index_name,
                               ix.indisunique AS is_unique,
                               pg_get_expr(ix.indpred, ix.indrelid) AS filter,
                               array_agg(a.attname ORDER BY k.ord) AS columns
                           FROM pg_index ix
                           JOIN pg_class i ON i.oid = ix.indexrelid
                           JOIN pg_class t ON t.oid = ix.indrelid
                           JOIN pg_namespace n ON n.oid = t.relnamespace
                           JOIN unnest(ix.indkey) WITH ORDINALITY AS k(attnum, ord) ON true
                           JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = k.attnum
                           WHERE n.nspname = @schema
                             AND NOT ix.indisprimary
                             AND t.relkind = 'r'
                           GROUP BY t.relname, i.relname, ix.indisunique, ix.indpred, ix.indrelid;
                           """;

        var result = new Dictionary<string, List<IndexSnapshot>>(StringComparer.OrdinalIgnoreCase);

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("schema", _schemaName);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var table = reader.GetString(0);
            var name = reader.GetString(1);
            var isUnique = reader.GetBoolean(2);
            var filter = reader.IsDBNull(3) ? null : reader.GetString(3);
            var columns = (string[])reader.GetValue(4);

            if (!result.TryGetValue(table, out var list))
            {
                list = new List<IndexSnapshot>();
                result[table] = list;
            }

            list.Add(new IndexSnapshot
            {
                Name = name,
                Columns = columns,
                IsUnique = isUnique,
                Filter = filter
            });
        }

        return result;
    }

    private Dictionary<string, List<ForeignKeySnapshot>> ReadForeignKeys()
    {
        const string sql = """
                           SELECT
                               t.relname AS table_name,
                               con.conname AS fk_name,
                               (SELECT array_agg(a.attname ORDER BY k.ord)
                                FROM unnest(con.conkey) WITH ORDINALITY k(attnum, ord)
                                JOIN pg_attribute a ON a.attrelid = con.conrelid AND a.attnum = k.attnum
                               ) AS columns,
                               pt.relname AS principal_table,
                               (SELECT array_agg(a.attname ORDER BY k.ord)
                                FROM unnest(con.confkey) WITH ORDINALITY k(attnum, ord)
                                JOIN pg_attribute a ON a.attrelid = con.confrelid AND a.attnum = k.attnum
                               ) AS principal_columns
                           FROM pg_constraint con
                           JOIN pg_class t ON t.oid = con.conrelid
                           JOIN pg_class pt ON pt.oid = con.confrelid
                           JOIN pg_namespace n ON n.oid = t.relnamespace
                           WHERE con.contype = 'f'
                             AND n.nspname = @schema;
                           """;

        var result = new Dictionary<string, List<ForeignKeySnapshot>>(StringComparer.OrdinalIgnoreCase);

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("schema", _schemaName);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var table = reader.GetString(0);
            var name = reader.GetString(1);
            var columns = (string[])reader.GetValue(2);
            var principalTable = reader.GetString(3);
            var principalColumns = (string[])reader.GetValue(4);

            if (!result.TryGetValue(table, out var list))
            {
                list = new List<ForeignKeySnapshot>();
                result[table] = list;
            }

            list.Add(new ForeignKeySnapshot
            {
                Name = name,
                Columns = columns,
                PrincipalTable = principalTable,
                PrincipalColumns = principalColumns
            });
        }

        return result;
    }
}