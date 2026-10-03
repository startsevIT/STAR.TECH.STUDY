using Application.Users.Models;
using Domain.Organisations;
using Domain.Users;
using PasswordHasher.Abstractions;
using Storage.Abstractions;
using Storage.Abstractions.Exceptions;
using TokenCreator.Abstractions;

namespace Application.Users;

public class UserService(
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    EmailConfirmationService emailConfirmationService,
    IStorage storage)
{
    public async Task Register(UserRegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var phone = NormalizePhone(request.Phone);

        var (hash, salt) = passwordHasher.Hash(request.Password);
        var userId = storage.Transaction(() =>
        {
            var exists = storage.Select<UserCredential>()
                .Any(x => x.Email == email || x.Phone == phone);

            if (exists)
                throw new EntityAlreadyExistException("User with same credentials already exists.");

            var userId = storage.Insert(User.Create(request.FirstName, request.LastName, request.Patronymic));
            storage.Insert(UserCredential.Create(userId, email, phone, hash, salt));
            return userId;
        });

        await emailConfirmationService.ResendAsync(userId);
    }

    public UserLoginResponse Login(UserLoginRequest request)
    {
        var login = request.Login.Contains('@')
            ? NormalizeEmail(request.Login)
            : NormalizePhone(request.Login);

        var credentials = storage.Select<UserCredential>()
            .Where(x => x.Email == login || x.Phone == login)
            .Take(2)
            .ToArray();

        if (credentials.Length > 1)
            throw new InvalidOperationException(
                $"Invariant violated: found {credentials.Length} credentials for login '{login}'.");

        var isValid = credentials.Length == 1
            ? credentials[0].Verify(request.Password, passwordHasher)
            : passwordHasher.FakeVerify(request.Password);

        if (credentials.Length == 0 || !isValid)
            throw new InvalidCredentialsException();

        var credential = credentials[0];

        if (!credential.IsEmailConfirmed)
            throw new EmailNotConfirmedException();

        var token = tokenService.CreateToken(credential.UserId);

        var accesses = storage.Select<UserOrganisation>()
            .Where(x => x.UserId == credential.UserId && x.IsActive)
            .GroupBy(x => x.OrganisationId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Role).ToArray());

        return new UserLoginResponse(token, accesses);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string NormalizePhone(string phone) => new(phone.Where(char.IsAsciiDigit).ToArray());
}