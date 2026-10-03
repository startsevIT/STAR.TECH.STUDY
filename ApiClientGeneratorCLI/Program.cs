using ApiClientGenerator.Abstractions.Abstractions;
using ApiClientGenerator.Flutter;
using ApiClientGenerator.React;

if (args.Length < 3)
{
    PrintUsage();
    return 1;
}

var languageTargetName = args[0].ToLowerInvariant();
var swaggerPath = args[1];
var outputDir = args[2];

var apiClientTarget = GetArgValue(args, "--target");
if (apiClientTarget is null)
{
    Console.Error.WriteLine("--target is required.");
    PrintUsage();
    return 1;
}

if (!File.Exists(swaggerPath))
{
    Console.Error.WriteLine($"File not found: {swaggerPath}");
    return 1;
}

ILanguageTarget languageTarget = languageTargetName switch
{
    "react" => new ReactLanguageTarget(),
    "flutter" => new FlutterLanguageTarget(),
    _ => throw new ArgumentException($"Unknown language target: {languageTargetName}")
};

var swaggerJson = await File.ReadAllTextAsync(swaggerPath);

var generator = new ApiClientGenerator.Abstractions.ApiClientGenerator(languageTarget);

try
{
    generator.Generate(swaggerJson, outputDir, apiClientTarget);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Generation failed: {ex.Message}");
    return 1;
}

return 0;

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

static void PrintUsage()
{
    Console.WriteLine("Usage: ApiClientGenerator.Cli <language> <swagger.json> <output-dir> --target <api-client-target>");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  language            — language target: react | flutter");
    Console.WriteLine("  swagger.json        — path to OpenAPI document");
    Console.WriteLine("  output-dir          — output directory");
    Console.WriteLine("  --target            — API client target: flutter-outer | react-inner | back-office");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  ApiClientGenerator.Cli flutter ./swagger.json ./generated/flutter --target flutter-outer");
    Console.WriteLine("  ApiClientGenerator.Cli react   ./swagger.json ./generated/react   --target react-inner");
}

//flutter ../../../../openapi/api-spec.json ../../../../../flutter_client/lib/auto_generated_api --target flutter-outer

//flutter ../../../../openapi/api-spec.json ../../../../../flutter_client/lib/auto_generated_api
