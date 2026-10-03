using ApiClientGenerator.Abstractions.Abstractions;
using ApiClientGenerator.Abstractions.Parsing;

namespace ApiClientGenerator.Abstractions;

public sealed class ApiClientGenerator(ILanguageTarget target)
{
    public void Generate(string swaggerJson, string outputDir, Action<string>? log = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(swaggerJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDir);

        log ??= Console.WriteLine;

        if (Directory.Exists(outputDir))
        {
            log($"Cleaning: {outputDir}");
            Directory.Delete(outputDir, recursive: true);
        }

        Directory.CreateDirectory(outputDir);

        var parsed = OpenApiParser.Parse(swaggerJson);

        log($"Endpoints: {parsed.Endpoints.Count}");
        log($"Schemas: {parsed.Schemas.Count}");

        var allTags = parsed.Endpoints
            .Select(e => e.Tag)
            .Distinct()
            .ToList();

        var modelsByTag = ModelUsageAnalyzer.ResolveForTags(parsed, allTags);
        var sharedModels = ModelUsageAnalyzer.FindShared(modelsByTag);

        var controllerPathByTag = parsed.Endpoints
            .GroupBy(e => e.Tag)
            .ToDictionary(g => g.Key, g => g.First().ControllerPath);

        log($"Tags (controllers): {allTags.Count}");

        foreach (var tag in allTags)
        {
            var path = controllerPathByTag[tag];
            var models = modelsByTag[tag].Count;
            var local = modelsByTag[tag].Count(m => !sharedModels.Contains(m));

            log($"  [{tag}] path='{path}' models={models} local={local}");
        }

        log($"Shared models: {sharedModels.Count}");

        var tagFolders = BuildTagFolders(allTags, controllerPathByTag);

        EmitModels(parsed, modelsByTag, sharedModels, tagFolders, outputDir);
        EmitRepositories(parsed, modelsByTag, sharedModels, tagFolders, outputDir, log);

        log($"Done: {outputDir}");
    }

    private Dictionary<string, string> BuildTagFolders(
        IEnumerable<string> allTags,
        Dictionary<string, string> controllerPathByTag)
    {
        var result = new Dictionary<string, string>();

        foreach (var tag in allTags)
        {
            var controllerPath = controllerPathByTag.GetValueOrDefault(tag, "");
            var folderName = target.Naming.FolderName(tag);

            var folder = string.IsNullOrWhiteSpace(controllerPath)
                ? folderName
                : $"{controllerPath}/{folderName}";

            result[tag] = folder;
        }

        return result;
    }

    private void EmitModels(
        ParsedApi parsed,
        Dictionary<string, HashSet<string>> modelsByTag,
        HashSet<string> sharedModels,
        Dictionary<string, string> tagFolders,
        string outputDir)
    {
        var emitter = target.ModelEmitter;

        if (sharedModels.Count > 0)
        {
            var content = emitter.Emit(sharedModels, parsed.Schemas);
            var path = Path.Combine(outputDir, $"models{emitter.FileExtension}");
            File.WriteAllText(path, content);
        }

        foreach (var (tag, models) in modelsByTag)
        {
            var localModels = models
                .Where(m => !sharedModels.Contains(m))
                .ToHashSet();

            if (localModels.Count == 0)
                continue;

            var folder = tagFolders[tag];

            var modelsDir = Path.Combine(
                outputDir,
                "repositories",
                folder.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(modelsDir);

            var content = emitter.Emit(localModels, parsed.Schemas);
            var path = Path.Combine(modelsDir, $"models{emitter.FileExtension}");
            File.WriteAllText(path, content);
        }
    }

    private void EmitRepositories(
        ParsedApi parsed,
        Dictionary<string, HashSet<string>> modelsByTag,
        HashSet<string> sharedModels,
        Dictionary<string, string> tagFolders,
        string outputDir,
        Action<string> log)
    {
        var emitter = target.RepositoryEmitter;

        // HTTP-клиент
        var httpPath = Path.Combine(outputDir, emitter.HttpClientFileName);
        File.WriteAllText(httpPath, emitter.EmitHttpClient());

        // Репозитории
        var repositoriesDir = Path.Combine(outputDir, "repositories");
        Directory.CreateDirectory(repositoriesDir);

        var groups = parsed.Endpoints
            .GroupBy(e => e.Tag)
            .Select(g => new
            {
                Tag = g.Key,
                Endpoints = g.ToList()
            })
            .ToList();

        foreach (var group in groups)
        {
            var folder = tagFolders[group.Tag];

            var folderPath = Path.Combine(
                repositoriesDir,
                folder.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(folderPath);

            var localModels = modelsByTag.TryGetValue(group.Tag, out var m)
                ? m.Where(x => !sharedModels.Contains(x)).ToHashSet()
                : [];

            var depth = 2 + folder.Count(c => c == '/');
            var up = string.Concat(Enumerable.Repeat("../", depth));

            var fileName = $"{target.Naming.FolderName(group.Tag)}_repository{emitter.FileExtension}";
            var content = emitter.EmitRepository(group.Tag, group.Endpoints, localModels, up);
            File.WriteAllText(Path.Combine(folderPath, fileName), content);
        }

        log($"Repositories: {groups.Count}");
    }
}