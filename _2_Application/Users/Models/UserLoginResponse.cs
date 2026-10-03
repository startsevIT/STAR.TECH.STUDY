using Domain.Users;

namespace Application.Users.Models;

public record UserLoginResponse(
    string Token, 
    Dictionary<Guid, Role[]> RolesByOrganisations);
    