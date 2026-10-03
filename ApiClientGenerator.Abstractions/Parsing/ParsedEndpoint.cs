using ApiClientGenerator.Abstractions.Models;

namespace ApiClientGenerator.Abstractions.Parsing;

public class ParsedEndpoint
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public required string OperationId { get; init; }
    public required string Tag { get; init; }
    public required string ControllerPath { get; init; }
    public required List<OpenApiParameter> Parameters { get; init; }
    public OpenApiSchema? RequestBody { get; init; }
    public OpenApiSchema? ResponseBody { get; init; }
    public IReadOnlyList<string> ApiClientTargets { get; init; } = [];
}