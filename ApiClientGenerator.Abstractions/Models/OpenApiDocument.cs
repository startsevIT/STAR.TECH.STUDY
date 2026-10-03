using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiDocument
{
    [JsonPropertyName("openapi")]
    public string OpenApi { get; set; } = "";

    [JsonPropertyName("paths")]
    public Dictionary<string, Dictionary<string, OpenApiOperation>> Paths { get; set; } = new();

    [JsonPropertyName("components")]
    public OpenApiComponents Components { get; set; } = new();
}