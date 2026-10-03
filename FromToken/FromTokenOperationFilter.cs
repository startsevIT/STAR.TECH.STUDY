using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FromToken;

internal sealed class FromTokenOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parametersWithFromToken = context.MethodInfo
            .GetParameters()
            .Where(p => p.GetCustomAttributes(typeof(FromTokenAttribute), inherit: true).Length != 0)
            .Select(p => p.Name)
            .ToHashSet();

        if (parametersWithFromToken.Count == 0)
            return;

        operation.Parameters = operation.Parameters
            .Where(p => !parametersWithFromToken.Contains(p.Name))
            .ToList();
    }
}