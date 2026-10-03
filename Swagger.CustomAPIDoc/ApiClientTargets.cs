namespace Swagger.CustomAPIDoc;

public static class ApiClientTargets
{
    private static readonly Lazy<IReadOnlyList<string>> _all = new(() =>
        typeof(ApiClientTargets).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.IsSubclassOf(typeof(ApiClientTargetAttribute)))
            .Select(t => (ApiClientTargetAttribute)Activator.CreateInstance(t)!)
            .Select(a => a.Target)
            .Distinct()
            .OrderBy(t => t)
            .ToList());

    public static IReadOnlyList<string> All => _all.Value;
}