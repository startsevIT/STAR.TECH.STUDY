using ApiClientGenerator.Abstractions.Models;

namespace ApiClientGenerator.Abstractions.Parsing;

public static class ModelUsageAnalyzer
{
    public static Dictionary<string, HashSet<string>> ResolveForTags(
        ParsedApi api,
        IEnumerable<string> allTags)
    {
        var result = new Dictionary<string, HashSet<string>>();

        foreach (var tag in allTags)
        {
            var modelsForTag = new HashSet<string>();
            var toProcess = new Queue<string>();

            foreach (var endpoint in api.Endpoints.Where(e => e.Tag == tag))
            {
                CollectRefs(endpoint.RequestBody, toProcess);
                CollectRefs(endpoint.ResponseBody, toProcess);
                foreach (var p in endpoint.Parameters)
                    CollectRefs(p.Schema, toProcess);
            }

            while (toProcess.Count > 0)
            {
                var modelName = toProcess.Dequeue();
                if (!modelsForTag.Add(modelName)) continue;
                if (!api.Schemas.TryGetValue(modelName, out var schema)) continue;
                CollectRefs(schema, toProcess);
            }

            result[tag] = modelsForTag;
        }

        return result;
    }

    public static HashSet<string> FindShared(Dictionary<string, HashSet<string>> modelsByTag)
    {
        var modelToTags = new Dictionary<string, HashSet<string>>();

        foreach (var (tag, models) in modelsByTag)
        {
            foreach (var m in models)
            {
                if (!modelToTags.TryGetValue(m, out var tags))
                {
                    tags = new HashSet<string>();
                    modelToTags[m] = tags;
                }
                tags.Add(tag);
            }
        }

        return modelToTags
            .Where(x => x.Value.Count > 1)
            .Select(x => x.Key)
            .ToHashSet();
    }

    private static void CollectRefs(OpenApiSchema? schema, Queue<string> queue)
    {
        if (schema is null) return;

        if (schema.Ref is not null)
            queue.Enqueue(schema.Ref.Split('/')[^1]);

        if (schema.Items is not null)
            CollectRefs(schema.Items, queue);

        if (schema.Properties is null) return;

        foreach (var p in schema.Properties.Values)
            CollectRefs(p, queue);
    }
}