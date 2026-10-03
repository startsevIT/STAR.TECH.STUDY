namespace ApiClientGenerator.Abstractions.Abstractions;

public interface ITypeMapper
{
    string MapPrimitive(string type, string? format);

    string MapArray(string elementType);

    string MapObject();

    string MapUnknown();

    string MakeNullable(string type);

    string MapReference(string modelName, bool isLocal);
}