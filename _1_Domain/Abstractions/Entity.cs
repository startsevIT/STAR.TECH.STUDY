using Storage.Abstractions;

namespace Domain.Abstractions;

public abstract class Entity : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}