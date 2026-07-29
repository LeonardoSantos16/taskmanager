using taskmanager.Models;

namespace taskmanager.Services
{
    public record TokenResult(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        TokenResult GenerateToken(User user);
    }
}
