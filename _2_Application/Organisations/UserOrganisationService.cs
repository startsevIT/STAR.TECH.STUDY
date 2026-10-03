using Application.Organisations.Models;
using Domain.Organisations;
using Domain.Users;
using Storage.Abstractions;

namespace Application.Organisations;

public class UserOrganisationService(IStorage storage)
{
    //todo ВНУТРЕННЕЕ ПРИЛОЖЕНИЕ. Администрация организации может добавлять пачками пользователей
    public void AddOrReactivateUser(Guid organisationId, UserOrganisationRequest request) =>
        AddOrReactivateUsers(organisationId, [request]);

    public void AddOrReactivateUsers(Guid organisationId, IReadOnlyList<UserOrganisationRequest> requests)
    {
        storage.Transaction(() =>
        {
            var distinctRequests = requests
                .GroupBy(x => (x.UserId, x.Role))
                .Select(g => g.First())
                .ToList();

            var userIds = distinctRequests.Select(x => x.UserId).Distinct().ToList();

            var existingLinks = storage.Select<UserOrganisation>()
                .Where(y => y.OrganisationId == organisationId)
                .Where(y => userIds.Contains(y.UserId))
                .ToList()
                .ToDictionary(y => (y.UserId, y.Role));

            foreach (var request in distinctRequests)
            {
                if (existingLinks.TryGetValue((request.UserId, request.Role), out var existingLink))
                {
                    if (existingLink.IsActive)
                        continue;

                    existingLink.SetIsActive(true);
                    storage.Update(existingLink);
                }
                else
                    storage.Insert(UserOrganisation.Create(request.UserId, organisationId, request.Role));
            }
        });
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

    public IReadOnlyList<OrganisationUserResponse> GetUsers(Guid organisationId)
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
            .Select(u => new OrganisationUserResponse(u.Id, u.FirstName, u.LastName, u.Patronymic, rolesByUserId[u.Id]))
            .ToList();
    }
}

