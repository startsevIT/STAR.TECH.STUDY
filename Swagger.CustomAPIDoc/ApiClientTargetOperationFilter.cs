using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swagger.CustomAPIDoc;

public sealed class ApiClientTargetOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;

        var targets = methodInfo
            .GetCustomAttributes(typeof(ApiClientTargetAttribute), inherit: true)
            .OfType<ApiClientTargetAttribute>()
            .Select(a => a.Target)
            .Distinct()
            .ToArray();

        if (targets.Length == 0)
            throw new InvalidOperationException(
                $"Method {methodInfo.DeclaringType?.Name}.{methodInfo.Name} " +
                $"has no [ApiClientTarget] attribute. " +
                $"Every action must be marked with at least one of: " +
                $"[ForFlutterOuterClient], [ForReactInnerClient], [ForBackOfficeClient].");

        var jsonArray = new JsonArray(
            targets.Select(JsonNode (t) => JsonValue.Create(t)!).ToArray());

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions["x-api-client-target"] = new JsonNodeExtension(jsonArray);
    }
}