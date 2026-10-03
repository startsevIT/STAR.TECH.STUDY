namespace SnakeCaseConverter;

public static class SnakeCaseNamingConverter
{
    public static string ToSnakeCase(string? name) =>
        string.IsNullOrEmpty(name)
            ? string.Empty
            : string.Concat(
                name.Select((ch, i) =>
                    i > 0 && char.IsUpper(ch) &&
                    (i + 1 < name.Length && char.IsLower(name[i + 1]) ||
                     i + 1 == name.Length ||
                     char.IsLower(name[i - 1]))
                        ? "_" + ch.ToString().ToLower()
                        : ch.ToString().ToLower()
                )
            );
}