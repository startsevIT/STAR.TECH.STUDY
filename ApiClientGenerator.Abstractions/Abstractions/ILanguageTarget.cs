namespace ApiClientGenerator.Abstractions.Abstractions;

public interface ILanguageTarget
{
    string Name { get; }

    ITypeMapper TypeMapper { get; }
    INamingConvention Naming { get; }
    IModelEmitter ModelEmitter { get; }
    IRepositoryEmitter RepositoryEmitter { get; }
}