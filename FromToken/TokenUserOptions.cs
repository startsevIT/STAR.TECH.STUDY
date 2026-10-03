namespace FromToken;

public sealed class TokenUserOptions
{
    internal Dictionary<Type, Func<string, object?>> Parsers { get; } = new();

    internal Dictionary<string, string[]> ClaimAliases { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public TokenUserOptions AddParser<T>(Func<string, T?> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        Parsers[typeof(T)] = value => parser(value);
        return this;
    }

    public TokenUserOptions AddClaimAlias(string parameterName, params string[] claimTypes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        ArgumentNullException.ThrowIfNull(claimTypes);

        ClaimAliases[parameterName] = claimTypes;
        return this;
    }
}