using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiRequestBody
{
    [JsonPropertyName("content")]
    public Dictionary<string, OpenApiMediaType> Content { get; set; } = new();
}