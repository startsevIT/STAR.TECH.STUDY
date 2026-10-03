using Domain.Organisations;
using Domain.Users;
using Storage.Abstractions;

namespace Application.Organisations;

public class OrganisationService(IStorage storage)
{
    public Guid RegisterOrganisation(Guid ownerId, OrganisationCreateRequest request) =>
        storage.Transaction(() =>
        {
            var id = storage.Insert(Organisation.Create(ownerId, request.Name, request.City));
            storage.Insert(UserOrganisation.Create(ownerId, id, Role.Owner));
            return id;
        });
}

public record OrganisationCreateRequest(
    string Name,
    string City);