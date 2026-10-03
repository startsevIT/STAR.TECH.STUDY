using Domain.Users;

namespace Application.Organisations.Models;

public sealed record OrganisationUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Patronymic,
    HashSet<Role> Roles);