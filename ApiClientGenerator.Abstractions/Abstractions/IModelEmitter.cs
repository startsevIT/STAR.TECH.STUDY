using ApiClientGenerator.Abstractions.Models;

namespace ApiClientGenerator.Abstractions.Abstractions;

public interface IModelEmitter
{
    string FileExtension { get; }

    string Emit(IReadOnlyCollection<string> models, IReadOnlyDictionary<string, OpenApiSchema> allSchemas);
}