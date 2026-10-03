namespace Application.Users.Models;

public record UserLoginRequest(
    string Login,
    string Password);