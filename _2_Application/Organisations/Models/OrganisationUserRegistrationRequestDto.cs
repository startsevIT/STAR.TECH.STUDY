using Domain.Organisations;
using Domain.Users;

namespace Application.Organisations.Models;

public sealed record OrganisationUserRegistrationRequestDto(
    Guid Id,
    Guid UserId,
    Guid OrganisationId,
    Role Role,
    RegistrationRequestStatus Status,
    Guid? ManagerId);