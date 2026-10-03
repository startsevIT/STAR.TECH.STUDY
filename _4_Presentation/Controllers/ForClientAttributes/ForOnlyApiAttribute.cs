using Swagger.CustomAPIDoc;

namespace Presentation.Controllers.ForClientAttributes;

public sealed class ForOnlyApiAttribute() : ApiClientTargetAttribute("only-api");