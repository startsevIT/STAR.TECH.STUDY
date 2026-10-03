using SnakeCaseConverter;

namespace Swagger.CustomAPIDoc;

public sealed class ForFlutterOuterClientAttribute : ApiClientTargetAttribute
{
    public ForFlutterOuterClientAttribute() : base("flutter-outer-client") { }
}

public sealed class ForReactInnerClientAttribute : ApiClientTargetAttribute
{
    public ForReactInnerClientAttribute() : base("react-inner-client") { }
}

public sealed class ForBackOfficeClientAttribute : ApiClientTargetAttribute
{
    public ForBackOfficeClientAttribute() : base("back-office-client") { }
}

public sealed class ForOnlyApiAttribute : ApiClientTargetAttribute
{
    public ForOnlyApiAttribute() : base("only-api") { }
}