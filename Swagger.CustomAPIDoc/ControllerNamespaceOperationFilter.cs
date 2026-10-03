using Microsoft.OpenApi;
using SnakeCaseConverter;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swagger.CustomAPIDoc;

public class ControllerNamespaceOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerType = context.MethodInfo.DeclaringType;
        if (controllerType is null) return;

        var ns = controllerType.Namespace ?? string.Empty;

        const string marker = ".Controllers";
        var idx = ns.IndexOf(marker, StringComparison.Ordinal);

        string path;
        if (idx >= 0)
        {
            var tail = ns[(idx + marker.Length)..].TrimStart('.');
            path = string.IsNullOrEmpty(tail) ? "" : SnakeCaseNamingConverter.ToSnakeCase(tail.Replace('.', '/'));
        }
        else
            path = "";

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions["x-controller-path"] = new JsonNodeExtension(path);
    }
}