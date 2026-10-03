using Domain.Abstractions;

namespace Domain.Organisations;

public class Organisation : Entity
{
    private Organisation(Guid ownerId, string name, string city)
    {
        Name = name;
        OwnerId = ownerId;
        City = city;
    }

    public string Name { get; private set; }
    public string City { get; private set; }
    public Guid OwnerId { get; private set; } 

    public static Organisation Create(Guid ownerId, string name, string city) => new(ownerId, name, city);
}