namespace Swagger.CustomAPIDoc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class ApiClientTargetAttribute(string target) : Attribute
{
    public string Target { get; } = target;
}