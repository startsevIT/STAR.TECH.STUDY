using ApiClientGenerator.Abstractions.Abstractions;
using SnakeCaseConverter;

namespace ApiClientGenerator.React;

public sealed class TypeScriptNamingConvention : INamingConvention
{
    public string MethodName(string operationId)
        => string.IsNullOrEmpty(operationId)
            ? "call"
            : char.ToLowerInvariant(operationId[0]) + operationId[1..];

    public string PropertyName(string name) => name;

    public string ClassName(string name) => name;

    public string EnumMemberName(string value)
    {
        var clean = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrEmpty(clean) ? "_" : clean;
    }

    public string FileName(string name) => ToSnakeCase(name);

    public string FolderName(string name) => ToSnakeCase(name);

    private static string ToSnakeCase(string value) =>
        string.IsNullOrEmpty(value) ? value : SnakeCaseNamingConverter.ToSnakeCase(value);
}