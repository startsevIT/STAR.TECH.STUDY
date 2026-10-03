using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace FromToken;

internal sealed class UserIdModelBinder(IOptions<TokenUserOptions> options) : IModelBinder
{
    private readonly TokenUserOptions _options = options.Value;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (!_options.Parsers.TryGetValue(bindingContext.ModelType, out var parser))
            parser = value => JsonSerializer.Deserialize(value, bindingContext.ModelType);

        var claimTypes = _options.ClaimAliases.TryGetValue(
            bindingContext.FieldName,
            out var aliases)
            ? aliases
            : [bindingContext.FieldName];

        var user = bindingContext.HttpContext.User;

        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirst(claimType)?.Value;

            if (value is null)
                continue;

            try
            {
                var parsed = parser(value);
                bindingContext.Result = ModelBindingResult.Success(parsed);
                return Task.CompletedTask;
            }
            catch
            {
                // Пробуем следующий claim.
            }
        }

        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}