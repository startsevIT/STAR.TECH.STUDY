using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiResponse
{
    [JsonPropertyName("content")]
    public Dictionary<string, OpenApiMediaType>? Content { get; set; }
}