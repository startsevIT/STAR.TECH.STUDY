using ApiClientGenerator.Abstractions.Abstractions;

namespace ApiClientGenerator.Flutter;

public sealed class DartTypeMapper : ITypeMapper
{
    public string MapPrimitive(string type, string? format) => type switch
    {
        "string" => format switch
        {
            "date-time" => "DateTime",
            "date" => "DateTime",
            "time" => "String",
            "uuid" => "String",
            "binary" => "List<int>",
            _ => "String"
        },
        "integer" => "int",
        "number" => "double",
        "boolean" => "bool",
        _ => "dynamic"
    };

    public string MapArray(string elementType) => $"List<{elementType}>";

    public string MapObject() => "Map<String, dynamic>";

    public string MapUnknown() => "dynamic";

    public string MakeNullable(string type) => $"{type}?";

    public string MapReference(string modelName, bool isLocal) => modelName;
}