using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace FromToken;

internal sealed class UserIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.BindingInfo.BindingSource != BindingSource.Custom)
            return null;

        return ActivatorUtilities.CreateInstance<UserIdModelBinder>(context.Services);
    }
}