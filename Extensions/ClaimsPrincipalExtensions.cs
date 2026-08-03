using System.Security.Claims;

namespace taskmanager.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Token does not contain a user id.");

            return Guid.Parse(value);
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email)
                ?? throw new UnauthorizedAccessException("Token does not contain an email.");
        }
    }
}
