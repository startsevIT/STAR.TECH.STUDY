using Application.Organisations.Models;
using Domain.Organisations;
using Domain.Users;
using Storage.Abstractions;

namespace Application.Organisations;

public class UserOrganisationService(IStorage storage)
{
    public Guid AddOrReactivateUser(Guid organisationId, Guid userId, Role role)
    {
        var existingLink = storage.Select<UserOrganisation>()
            .SingleOrDefault(x => x.UserId == userId && x.OrganisationId == organisationId && x.Role == role);

        if (existingLink == null)
            return storage.Insert(UserOrganisation.Create(userId, organisationId, role));

        if (!existingLink.IsActive)
        {
            existingLink.SetIsActive(true);
            storage.Update(existingLink);
        }

        return existingLink.Id;
    }

    public void RemoveUser(Guid organisationId, Guid userId, Role role)
    {
        var link = storage.SingleOrThrow<UserOrganisation>(x =>
            x.UserId == userId && x.OrganisationId == organisationId && x.Role == role);

        if (!link.IsActive)
            return;

        link.SetIsActive(false);
        storage.Update(link);
    }

    public IReadOnlyList<OrganisationUserDto> GetUsers(Guid organisationId)
    {
        var links = storage.Select<UserOrganisation>()
            .Where(x => x.OrganisationId == organisationId)
            .ToList();

        if (links.Count == 0)
            return [];

        var rolesByUserId = links
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Role).ToHashSet());

        var userIds = rolesByUserId.Keys.ToHashSet();

        return storage.Select<User>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new OrganisationUserDto(u.Id, u.FirstName, u.LastName, u.Patronymic, rolesByUserId[u.Id]))
            .ToList();
    }
}