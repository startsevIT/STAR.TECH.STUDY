using System.Linq.Expressions;

namespace Storage.Abstractions;

public interface IStorage
{
    Guid Insert<T>(T entity) where T : class, IEntity;
    IQueryable<T> Select<T>() where T : class, IEntity;
    T SingleOrThrow<T>(Expression<Func<T,bool>> func, string? notFoundMessage = null) where T : class, IEntity;
    
    T GetById<T>(Guid id) where T : class, IEntity;
    void Update<T>(T entity) where T : class, IEntity;
    void Delete<T>(Guid id) where T : class, IEntity;
    void DeleteWhere<T>(Expression<Func<T,bool>> func) where T : class, IEntity;
    void Transaction(Action action);
    T Transaction<T>(Func<T> action);
}