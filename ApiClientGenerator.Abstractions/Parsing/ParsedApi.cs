using ApiClientGenerator.Abstractions.Models;

namespace ApiClientGenerator.Abstractions.Parsing;

public class ParsedApi
{
    public required OpenApiDocument Document { get; init; }
    public required List<ParsedEndpoint> Endpoints { get; init; }
    public required Dictionary<string, OpenApiSchema> Schemas { get; init; }
}