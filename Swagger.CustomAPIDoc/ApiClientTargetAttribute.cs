namespace Swagger.CustomAPIDoc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class ApiClientTargetAttribute : Attribute
{
    protected ApiClientTargetAttribute(string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target);
        Target = target;
    }

    public string Target { get; }
}