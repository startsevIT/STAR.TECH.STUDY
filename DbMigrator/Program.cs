using Domain.Abstractions;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Storage.Migrations;

// Аргументы:
//   --check    — проверить, exit 1 если не up-to-date
//   --apply    — применить (по умолчанию)
//   --path     — путь к appsettings.json

var checkOnly = args.Contains("--check");
var configPath = ResolveConfigPath(GetArgValue(args, "--path"));

var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.GetDirectoryName(configPath)!)
    .AddJsonFile(Path.GetFileName(configPath), optional: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException(
                           "Connection string 'DefaultConnection' not found.");

var options = new StorageMigratorOptions
{
    SchemaName = "public",
    IgnoreStubs = false,
    UnsafeIndexRowsCountLimit = 10_000,
    UnsafeTablesRowsCountLimit = 10_000
};
var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var context = new AppDbContext(dbOptions);

if (context.Database.EnsureCreated())
    return;

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();


var migrator = new StorageMigrator(connection, context, options);
migrator.Migrate();

try
{
    if (checkOnly)
    {
        var pending = migrator.Check();

        if (pending.Count > 0)
        {
            Console.Error.WriteLine(
                $"Schema is not up to date. {pending.Count} pending change(s):");
            foreach (var ddl in pending)
                Console.Error.WriteLine($"  {ddl}");
            Environment.Exit(1);
        }

        Console.WriteLine("Schema is up to date.");
        Environment.Exit(0);
    }

    migrator.Migrate();
    Console.WriteLine("Schema updated successfully.");
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Migration failed: {ex.Message}");
    Environment.Exit(1);
}

static string? GetArgValue(string[] args, string name)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i].StartsWith($"{name}=", StringComparison.Ordinal))
            return args[i][(name.Length + 1)..];
        if (args[i] == name && i + 1 < args.Length)
            return args[i + 1];
    }

    return null;
}

static string ResolveConfigPath(string? argPath)
{
    if (argPath is null)
        return Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    if (Directory.Exists(argPath))
        return Path.Combine(argPath, "appsettings.json");

    if (!Path.IsPathRooted(argPath))
        return Path.Combine(AppContext.BaseDirectory, argPath);

    return argPath;
}