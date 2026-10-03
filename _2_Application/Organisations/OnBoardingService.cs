/*using System.Text.RegularExpressions;
using Domain.Organisations;
using Domain.Users;
using EmailSender.Abstractions;
using PasswordHasher.Abstractions;
using Storage.Abstractions;

namespace Application.Organisations;
todo
public class OnBoardingService(
    IStorage storage,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender)
{
    public void OnBoardingStudents(Guid managerUserId, Guid groupId, StudentRegisterRequest[] requests)
    {
        var distinct = requests
            .GroupBy(x => NormalizeEmail(x.Email))
            .Select(g => g.First())
            .ToArray();

        var group = storage.GetById<Group>(groupId);
        var organisationId = group.OrganisationId;

        var managerLinkId = GetActiveManagerLinkId(managerUserId, organisationId);

        var emailsToSend = new List<PendingEmail>();

        storage.Transaction(() =>
        {
            var emails = distinct.Select(x => NormalizeEmail(x.Email)).ToHashSet();

            var existingCredentials = storage.Select<UserCredential>()
                .Where(c => emails.Contains(c.Email))
                .ToList()
                .ToDictionary(c => c.Email);

            var existingUserIds = existingCredentials.Values
                .Select(c => c.UserId)
                .ToHashSet();

            var existingLinks = storage.Select<UserOrganisation>()
                .Where(uo => uo.OrganisationId == organisationId
                             && uo.Role == Role.Student
                             && existingUserIds.Contains(uo.UserId))
                .ToList()
                .ToDictionary(uo => uo.UserId);

            var pendingRequests = storage.Select<OrganisationUserRegistrationRequest>()
                .Where(r => r.OrganisationId == organisationId
                            && r.Role == Role.Student
                            && r.Status == RegistrationRequestStatus.Pending
                            && existingUserIds.Contains(r.UserId))
                .ToList()
                .ToDictionary(r => r.UserId);

            foreach (var student in distinct)
            {
                OnboardSingleStudent(
                    managerLinkId,
                    organisationId,
                    groupId,
                    student,
                    existingCredentials,
                    existingLinks,
                    pendingRequests,
                    emailsToSend);
            }
        });

        foreach (var email in emailsToSend)
            _ = emailSender.SendAsync(email.To, email.Subject, email.Body);
    }

    private void OnboardSingleStudent(
        Guid managerLinkId,
        Guid organisationId,
        Guid groupId,
        StudentRegisterRequest student,
        Dictionary<string, UserCredential> existingCredentials,
        Dictionary<Guid, UserOrganisation> existingLinks,
        Dictionary<Guid, OrganisationUserRegistrationRequest> pendingRequests,
        List<PendingEmail> emailsToSend)
    {
        var email = NormalizeEmail(student.Email);

        // ── 1. Есть ли уже учётка ──
        Guid userId;
        var isNewUser = !existingCredentials.TryGetValue(email, out var credential);

        if (isNewUser)
        {
            userId = storage.Insert(User.Create(
                student.FirstName,
                student.LastName,
                student.Patronymic ?? string.Empty));

            var password = PasswordGenerator.Generate();

            storage.Insert(UserCredential.CreateFromOnboarding(
                userId, email, student.Phone, password, passwordHasher));

            emailsToSend.Add(new PendingEmail(
                To: email,
                Subject: "Добро пожаловать в Study API",
                Body: $"""
                       Здравствуйте, {student.FirstName}!

                       Вы были добавлены в организацию.
                       Ваш логин: {email}
                       Ваш пароль: {password}

                       Пожалуйста, смените пароль после первого входа.
                       """));
        }
        else
        {
            userId = credential!.UserId;
        }

        // ── 2. Есть ли pending заявка — одобрить ──
        if (pendingRequests.TryGetValue(userId, out var pending))
        {
            pending.Approve(managerLinkId);
            storage.Update(pending);
        }

        // ── 3. UserOrganisation ──
        Guid userOrganisationId;

        if (existingLinks.TryGetValue(userId, out var existingLink))
        {
            userOrganisationId = existingLink.Id;

            if (!existingLink.IsActive)
            {
                existingLink.SetIsActive(true);
                storage.Update(existingLink);
            }
        }
        else
        {
            userOrganisationId = storage.Insert(
                UserOrganisation.Create(userId, organisationId, Role.Student));
        }

        // ── 4. GroupMember ──
        var existingMember = storage.Select<GroupMember>()
            .SingleOrDefault(m => m.GroupId == groupId
                                  && m.StudentUserOrganisationId == userOrganisationId);

        if (existingMember is null)
        {
            storage.Insert(GroupMember.Create(organisationId, groupId, userOrganisationId));

            // Письмо «вы добавлены в группу» — только для **существующего** пользователя
            // (новому уже ушло письмо с паролем выше).
            if (!isNewUser)
            {
                emailsToSend.Add(new PendingEmail(
                    To: email,
                    Subject: "Вы добавлены в новую группу",
                    Body: $"Здравствуйте, {student.FirstName}!\n\n" +
                          $"Вы зачислены в группу \"{GetGroupName(groupId)}\"."));
            }
        }
        else if (!existingMember.IsActive)
        {
            existingMember.Return();
            storage.Update(existingMember);
        }
        // Если active — ничего не делаем.
    }

    private Guid GetActiveManagerLinkId(Guid managerUserId, Guid organisationId)
    {
        var link = storage.Select<UserOrganisation>()
            .FirstOrDefault(uo => uo.UserId == managerUserId
                                  && uo.OrganisationId == organisationId
                                  && uo.IsActive);

        if (link is null)
            throw new ForbiddenException(
                $"Пользователь {managerUserId} не состоит в организации {organisationId}.");

        return link.Id;
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private sealed record PendingEmail(string To, string Subject, string Body);
}

public record StudentRegisterRequest(
    string FirstName,
    string LastName,
    string? Patronymic,
    string Phone,
    string Email);*/