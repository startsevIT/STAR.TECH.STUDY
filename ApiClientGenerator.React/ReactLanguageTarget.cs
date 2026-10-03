using ApiClientGenerator.Abstractions.Abstractions;

namespace ApiClientGenerator.React;

public sealed class ReactLanguageTarget : ILanguageTarget
{
    public string Name => "react";

    public ITypeMapper TypeMapper { get; }
    public INamingConvention Naming { get; }
    public IModelEmitter ModelEmitter { get; }
    public IRepositoryEmitter RepositoryEmitter { get; }

    public ReactLanguageTarget()
    {
        TypeMapper = new TypeScriptTypeMapper();
        Naming = new TypeScriptNamingConvention();
        ModelEmitter = new TypeScriptModelEmitter(TypeMapper);
        RepositoryEmitter = new TypeScriptRepositoryEmitter(TypeMapper, Naming);
    }
}