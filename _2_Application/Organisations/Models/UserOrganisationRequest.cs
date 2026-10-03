using Domain.Users;

namespace Application.Organisations.Models;

public record UserOrganisationRequest(Guid UserId, Role Role);