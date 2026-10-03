using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiComponents
{
    [JsonPropertyName("schemas")]
    public Dictionary<string, OpenApiSchema> Schemas { get; set; } = new();
}