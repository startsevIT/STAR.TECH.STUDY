using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiClientGenerator.Abstractions.Models;

public class OpenApiSchema
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("$ref")]
    public string? Ref { get; set; }

    [JsonPropertyName("items")]
    public OpenApiSchema? Items { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, OpenApiSchema>? Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }

    [JsonPropertyName("enum")]
    public List<JsonElement>? Enum { get; set; }

    [JsonPropertyName("nullable")]
    public bool Nullable { get; set; }
}