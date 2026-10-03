using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiMediaType
{
    [JsonPropertyName("schema")]
    public OpenApiSchema Schema { get; set; } = new();
}