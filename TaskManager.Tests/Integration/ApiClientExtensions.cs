using System.Net.Http.Headers;
using System.Net.Http.Json;
using taskmanager.DTOs;

namespace TaskManager.Tests.Integration;

public static class ApiClientExtensions
{
    public const string DefaultPassword = "Password1!";

    public static async Task<(Guid UserId, string Token, string Email)> RegisterAndLoginAsync(
        this HttpClient client, string? name = null, string? email = null, string password = DefaultPassword)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        name ??= $"User-{suffix}";
        email ??= $"user-{suffix}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Name = name,
            Email = email,
            Password = password
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return (auth!.User.Id, auth.Token, email);
    }

    public static void AuthenticateAs(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
