using System.Text.Json;
using System.Text.Json.Serialization;
using ApiClientGenerator.Abstractions.Models;

namespace ApiClientGenerator.Abstractions.Parsing;

public static class OpenApiParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static ParsedApi Parse(string json)
    {
        var doc = JsonSerializer.Deserialize<OpenApiDocument>(json, Options)
                  ?? throw new InvalidOperationException("Failed to parse OpenAPI document.");

        var endpoints = new List<ParsedEndpoint>();

        foreach (var (path, methods) in doc.Paths)
        {
            foreach (var (method, op) in methods)
            {
                if (method is "parameters" or "summary" or "description") continue;

                var tag = op.Tags.FirstOrDefault() ?? "Default";

                var requestSchema = op.RequestBody?.Content
                    .FirstOrDefault(x => x.Key == "application/json").Value?.Schema;

                var responseSchema = op.Responses
                    .Where(r => r.Key.StartsWith('2'))
                    .SelectMany(r => r.Value.Content ?? new Dictionary<string, OpenApiMediaType>())
                    .FirstOrDefault(x => x.Key == "application/json").Value?.Schema;

                endpoints.Add(new ParsedEndpoint
                {
                    Method = method.ToUpperInvariant(),
                    Path = path,
                    OperationId = op.OperationId ?? GenerateName(method, path),
                    Tag = tag,
                    ControllerPath = op.ControllerPath ?? "",
                    Parameters = op.Parameters,
                    RequestBody = requestSchema,
                    ResponseBody = responseSchema
                });
            }
        }

        return new ParsedApi
        {
            Document = doc,
            Endpoints = endpoints,
            Schemas = doc.Components.Schemas
        };
    }

    private static string GenerateName(string method, string path)
    {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('{', '}'))
            .Select(p => char.ToUpperInvariant(p[0]) + p[1..].Replace("-", "").Replace("_", ""));

        return method.ToLowerInvariant() + string.Concat(parts);
    }
}