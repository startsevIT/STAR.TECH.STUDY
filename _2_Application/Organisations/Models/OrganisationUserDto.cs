using Domain.Users;

namespace Application.Organisations.Models;

public sealed record OrganisationUserDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Patronymic,
    HashSet<Role> Roles);