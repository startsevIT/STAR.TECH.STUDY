namespace TokenCreator.Abstractions;

public interface ITokenService
{
    string CreateToken(Guid userId);
}