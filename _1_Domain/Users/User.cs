using Domain.Abstractions;

namespace Domain.Users;

public class User : Entity
{
    private User(string firstName, string lastName, string patronymic)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Patronymic { get; private set; }

    public static User Create(string firstName, string lastName, string patronymic) 
        => new(firstName, lastName, patronymic);
}

//todo register
//todo login