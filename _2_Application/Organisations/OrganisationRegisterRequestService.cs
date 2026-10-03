using Application.Organisations.Models;
using Domain.Organisations;
using Domain.Users;
using Storage.Abstractions;

namespace Application.Organisations;

public class OrganisationRegisterRequestService(
    UserOrganisationService userOrganisationService,
    IStorage storage)
{
    public Guid Create(Guid userId, Guid organisationId, Role role)
    {
        var alreadyMember = storage.Select<UserOrganisation>()
            .Any(x => x.UserId == userId
                      && x.OrganisationId == organisationId
                      && x.IsActive);

        if (alreadyMember)
            throw new InvalidOperationException(
                "User is already a member of this organisation.");

        var hasPending = storage.Select<OrganisationUserRegistrationRequest>()
            .Any(x => x.UserId == userId
                      && x.OrganisationId == organisationId
                      && x.Status == RegistrationRequestStatus.Pending);

        if (hasPending)
            throw new InvalidOperationException(
                "User already has a pending registration request for this organisation.");

        var request = OrganisationUserRegistrationRequest.Create(userId, organisationId, role);
        storage.Insert(request);
        return request.Id;
    }

    public void Approve(Guid requestId, Guid managerId)
    {
        var request = storage.GetById<OrganisationUserRegistrationRequest>(requestId);

        EnsureManagerOfOrganisation(managerId, request.OrganisationId);

        storage.Transaction(() =>
        {
            request.Approve(managerId);
            storage.Update(request);
            userOrganisationService.AddOrReactivateUser(request.OrganisationId,
                new UserOrganisationRequest(request.UserId, request.Role));
        });
    }

    public void Reject(Guid requestId, Guid managerId)
    {
        var request = storage.GetById<OrganisationUserRegistrationRequest>(requestId);

        EnsureManagerOfOrganisation(managerId, request.OrganisationId);

        request.Reject(managerId);
        storage.Update(request);
    }

    public IReadOnlyList<OrganisationUserRegistrationRequestDto> GetPending(Guid organisationId, Guid managerId)
    {
        EnsureManagerOfOrganisation(managerId, organisationId);

        return storage.Select<OrganisationUserRegistrationRequest>()
            .Where(x => x.OrganisationId == organisationId && x.Status == RegistrationRequestStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Select(x =>
                new OrganisationUserRegistrationRequestDto(x.Id, x.UserId, x.OrganisationId, x.Role, x.Status,
                    x.ManagerId))
            .ToList();
    }

    private void EnsureManagerOfOrganisation(Guid managerId, Guid organisationId)
    {
        var isManager = storage.Select<UserOrganisation>()
            .Any(x => x.UserId == managerId
                      && x.OrganisationId == organisationId
                      && x.Role == Role.Manager
                      && x.IsActive);

        if (!isManager)
            throw new InvalidOperationException(
                "Only a manager of this organisation can process registration requests.");
    }
}