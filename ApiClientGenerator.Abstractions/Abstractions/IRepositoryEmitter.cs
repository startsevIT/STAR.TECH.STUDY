using ApiClientGenerator.Abstractions.Parsing;

namespace ApiClientGenerator.Abstractions.Abstractions;

public interface IRepositoryEmitter
{
    string FileExtension { get; }

    string HttpClientFileName { get; }

    string EmitHttpClient();

    string EmitRepository(
        string tag,
        IReadOnlyList<ParsedEndpoint> endpoints,
        IReadOnlySet<string> localModels,
        string relativeImportPrefix);
}