using ApiClientGenerator.Abstractions.Abstractions;

namespace ApiClientGenerator.Flutter;

public sealed class DartNamingConvention : INamingConvention
{
    public string MethodName(string operationId)
        => string.IsNullOrEmpty(operationId)
            ? "call"
            : char.ToLowerInvariant(operationId[0]) + operationId[1..];

    public string PropertyName(string name) => ToCamelCase(name);

    public string ClassName(string name) => ToPascalCase(name);

    public string EnumMemberName(string value)
    {
        var clean = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrEmpty(clean) ? "_" : clean;
    }

    public string FileName(string name) => ToSnakeCase(name);

    public string FolderName(string name) => ToSnakeCase(name);

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var pascal = ToPascalCase(value);
        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var parts = value.Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p =>
            char.ToUpperInvariant(p[0]) + p[1..]));
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var sb = new System.Text.StringBuilder(value.Length + 4);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(value[i - 1]))
                    sb.Append('_');

                sb.Append(char.ToLowerInvariant(c));
            }
            else if (c is '-' or ' ')
                sb.Append('_');
            else
                sb.Append(c);
        }

        return sb.ToString();
    }
}