using Domain.Abstractions;
using Domain.Users;

namespace Domain.Organisations;

public sealed class OrganisationUserRegistrationRequest : Entity
{
    private OrganisationUserRegistrationRequest(
        Guid userId,
        Guid organisationId,
        Role role)
    {
        UserId = userId;
        OrganisationId = organisationId;
        Role = role;
        Status = RegistrationRequestStatus.Pending;
    }

    public Guid UserId { get; private init; }
    public Guid OrganisationId { get; private init; }
    public Role Role { get; private init; }
    public RegistrationRequestStatus Status { get; private set; }
    public Guid? ManagerId { get; private set; }

    public static OrganisationUserRegistrationRequest Create(
        Guid userId,
        Guid organisationId,
        Role role)
    {
        if (role is not (Role.Teacher or Role.Student))
            throw new ArgumentException(
                $"Role {role} cannot be requested. Only Teacher or Student.",
                nameof(role));

        return new OrganisationUserRegistrationRequest(userId, organisationId, role);
    }

    public void Approve(Guid managerId)
    {
        if (Status != RegistrationRequestStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot approve request in status {Status}.");

        Status = RegistrationRequestStatus.Approved;
        ManagerId = managerId;
    }

    public void Reject(Guid managerId)
    {
        if (Status != RegistrationRequestStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject request in status {Status}.");

        Status = RegistrationRequestStatus.Rejected;
        ManagerId = managerId;
    }
}