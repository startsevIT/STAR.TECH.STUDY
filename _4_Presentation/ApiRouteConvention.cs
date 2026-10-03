using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SnakeCaseConverter;

namespace Presentation;

public class ApiRouteConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var hasExplicitRoute = controller.Attributes
                .OfType<RouteAttribute>()
                .Any();

            if (hasExplicitRoute)
                continue;

            controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel(
                new RouteAttribute($"api/{SnakeCaseNamingConverter.ToSnakeCase(controller.ControllerName)}"));
        }
    }
}