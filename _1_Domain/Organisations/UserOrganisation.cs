using Domain.Abstractions;
using Domain.Users;

namespace Domain.Organisations;

public class UserOrganisation : Entity
{
    private UserOrganisation(Guid userId, Guid organisationId, Role role)
    {
        UserId = userId;
        OrganisationId = organisationId;
        Role = role;
        IsActive = true;
    }

    public Guid UserId { get; private set; }
    public Guid OrganisationId { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }

    public static UserOrganisation Create(Guid userId, Guid institutionId, Role role) 
        => new(userId, institutionId, role);
    
    public void SetIsActive(bool value) => IsActive = value;
}