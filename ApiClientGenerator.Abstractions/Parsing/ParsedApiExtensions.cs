namespace ApiClientGenerator.Abstractions.Parsing;

public static class ParsedApiExtensions
{
    public static ParsedApi FilterByApiClientTarget(this ParsedApi api, string target)
    {
        var filtered = api.Endpoints
            .Where(e => e.ApiClientTargets.Contains(target))
            .ToList();

        if (filtered.Count == 0)
            throw new InvalidOperationException(
                $"No endpoints found for API client target '{target}'. " +
                $"Check that controllers are marked with the corresponding attribute " +
                $"in the API project, and that the swagger.json contains " +
                $"'x-api-client-target' for those endpoints.");

        return new ParsedApi
        {
            Document = api.Document,
            Endpoints = filtered,
            Schemas = api.Schemas
        };
    }
}