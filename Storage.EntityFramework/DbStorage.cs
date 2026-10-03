using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Storage.Abstractions;
using Storage.Abstractions.Exceptions;

namespace Storage.EntityFramework;

public sealed class DbStorage(DbContext dbContext) : IStorage
{
    public Guid Insert<T>(T entity) where T : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        var set = dbContext.Set<T>();

        if (set.Local.Any(e => e.Id == entity.Id) ||
            set.Any(e => e.Id == entity.Id))
            throw new EntityAlreadyExistException(
                $"Entity {typeof(T).Name} with id {entity.Id} already exists.");

        set.Add(entity);
        dbContext.SaveChanges();
        return entity.Id;
    }

    public IQueryable<T> Select<T>() where T : class, IEntity
        => dbContext.Set<T>().AsNoTracking();

    public T SingleOrThrow<T>(Expression<Func<T, bool>> func, string? notFoundMessage = null) where T : class, IEntity =>
        dbContext.Set<T>().SingleOrDefault(func) ??
        throw new EntityNotFoundException(notFoundMessage ?? typeof(T).Name + " not found.");

    public T GetById<T>(Guid id) where T : class, IEntity
        => dbContext.Set<T>().FirstOrDefault(e => e.Id == id)
           ?? throw NotFound<T>(id);

    public void Update<T>(T entity) where T : class, IEntity
    {
        var set = dbContext.Set<T>();

        var existing = set.Local.FirstOrDefault(e => e.Id == entity.Id)
                       ?? set.FirstOrDefault(e => e.Id == entity.Id)
                       ?? throw NotFound<T>(entity.Id);

        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        dbContext.SaveChanges();
    }

    public void Delete<T>(Guid id) where T : class, IEntity
    {
        var set = dbContext.Set<T>();

        var existing = set.Local.FirstOrDefault(e => e.Id == id)
                       ?? set.FirstOrDefault(e => e.Id == id)
                       ?? throw NotFound<T>(id);

        set.Remove(existing);
        dbContext.SaveChanges();
    }

    public void DeleteWhere<T>(Expression<Func<T, bool>> func) where T : class, IEntity
    {
        var target = dbContext.Set<T>().Where(func);
        dbContext.Set<T>().RemoveRange(target);
        dbContext.SaveChanges();
    }

    public void Transaction(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        strategy.Execute(() =>
        {
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                action();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                dbContext.ChangeTracker.Clear();
                throw;
            }
        });
    }

    public T Transaction<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.Execute(() =>
        {
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                var result = action();
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                dbContext.ChangeTracker.Clear();
                throw;
            }
        });
    }

    private static EntityNotFoundException NotFound<T>(Guid id) =>
        new($"Entity {typeof(T).Name} with id {id} not found.");
}