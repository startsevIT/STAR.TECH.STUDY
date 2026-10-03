using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiOperation
{
    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("parameters")]
    public List<OpenApiParameter> Parameters { get; set; } = new();

    [JsonPropertyName("requestBody")]
    public OpenApiRequestBody? RequestBody { get; set; }

    [JsonPropertyName("responses")]
    public Dictionary<string, OpenApiResponse> Responses { get; set; } = new();

    [JsonPropertyName("x-controller-path")]
    public string? ControllerPath { get; set; }
    
    [JsonPropertyName("x-api-client-target")]
    public List<string> ApiClientTargets { get; set; } = new();
}