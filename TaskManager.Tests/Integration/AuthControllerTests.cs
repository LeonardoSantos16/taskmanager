using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using taskmanager.DTOs;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Integration;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(MsSqlContainerFixture sqlFixture) : base(sqlFixture)
    {
    }

    private record RegisterRequestDto(string Name, string Email, string Password);

    private static RegisterRequestDto BuildRegisterRequest(string? name = null, string? email = null, string? password = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new RegisterRequestDto(
            name ?? $"User-{suffix}",
            email ?? $"user-{suffix}@example.com",
            password ?? ApiClientExtensions.DefaultPassword);
    }

    [Fact]
    public async Task Register_ValidData_ReturnsNoContent()
    {
        // Arrange
        var request = BuildRegisterRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = BuildRegisterRequest();
        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = BuildRegisterRequest(password: "weak");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = BuildRegisterRequest();
        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { request.Email, request.Password });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrEmpty();
        auth.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_DoesNotSucceed()
    {
        // Arrange
        var request = BuildRegisterRequest();
        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { request.Email, Password = "WrongPassword1!" });

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Login_UnknownEmail_DoesNotSucceed()
    {
        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new { Email = "nobody@example.com", Password = ApiClientExtensions.DefaultPassword });

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }
}
