namespace Application.Users.Models;

public record UserRegisterRequest(
    string FirstName,
    string LastName,
    string Patronymic,
    string Email,
    string Phone,
    string Password);