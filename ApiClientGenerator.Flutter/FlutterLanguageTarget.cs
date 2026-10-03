using ApiClientGenerator.Abstractions.Abstractions;

namespace ApiClientGenerator.Flutter;

public sealed class FlutterLanguageTarget : ILanguageTarget
{
    public string Name => "flutter";

    public ITypeMapper TypeMapper { get; }
    public INamingConvention Naming { get; }
    public IModelEmitter ModelEmitter { get; }
    public IRepositoryEmitter RepositoryEmitter { get; }

    public FlutterLanguageTarget()
    {
        TypeMapper = new DartTypeMapper();
        Naming = new DartNamingConvention();
        ModelEmitter = new DartModelEmitter(TypeMapper);
        RepositoryEmitter = new DartRepositoryEmitter(TypeMapper, Naming);
    }
}