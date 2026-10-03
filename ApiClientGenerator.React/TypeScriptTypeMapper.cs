using ApiClientGenerator.Abstractions.Abstractions;

namespace ApiClientGenerator.React;

public sealed class TypeScriptTypeMapper : ITypeMapper
{
    public string MapPrimitive(string type, string? format) => type switch
    {
        "string" => format switch
        {
            "date-time" => "string",
            "date" => "string",
            "time" => "string",
            "uuid" => "string",
            "binary" => "Blob",
            _ => "string"
        },
        "integer" or "number" => "number",
        "boolean" => "boolean",
        _ => "unknown"
    };

    public string MapArray(string elementType) => $"{elementType}[]";

    public string MapObject() => "Record<string, unknown>";

    public string MapUnknown() => "unknown";

    public string MakeNullable(string type) => $"{type} | null";

    public string MapReference(string modelName, bool isLocal) =>
        isLocal ? $"Models.{modelName}" : modelName;
}