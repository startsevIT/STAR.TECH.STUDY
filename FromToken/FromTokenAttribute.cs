using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FromToken;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromTokenAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Custom;
}