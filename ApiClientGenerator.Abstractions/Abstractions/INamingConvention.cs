namespace ApiClientGenerator.Abstractions.Abstractions;

public interface INamingConvention
{
    string MethodName(string operationId);

    string PropertyName(string name);

    string ClassName(string name);

    string EnumMemberName(string value);

    string FileName(string name);

    string FolderName(string name);
}