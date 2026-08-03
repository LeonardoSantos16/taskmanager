using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using taskmanager.DTOs;
using taskmanager.Models;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Integration;

public class ProjectControllerTests : IntegrationTestBase
{
    public ProjectControllerTests(MsSqlContainerFixture sqlFixture) : base(sqlFixture)
    {
    }

    private static object BuildProjectRequest(string? name = null) => new
    {
        Name = name ?? $"Project-{Guid.NewGuid():N}"[..20],
        Description = (string?)null,
        Status = EnumProjectStatus.Active,
        OwnerId = Guid.Empty
    };

    [Fact]
    public async Task CreateProject_NoToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/project", BuildProjectRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_ValidToken_ReturnsCreatedAndOwnerCanFetchIt()
    {
        // Arrange
        var (_, token, _) = await Client.RegisterAndLoginAsync();
        Client.AuthenticateAs(token);

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/project", BuildProjectRequest());

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDtoResponse>();

        var getResponse = await Client.GetAsync($"/api/project/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProject_UserNotMember_ReturnsForbidden()
    {
        // Arrange
        var (_, ownerToken, _) = await Client.RegisterAndLoginAsync();
        Client.AuthenticateAs(ownerToken);
        var createResponse = await Client.PostAsJsonAsync("/api/project", BuildProjectRequest());
        var project = await createResponse.Content.ReadFromJsonAsync<ProjectDtoResponse>();

        using var outsiderClient = CreateAdditionalClient();
        var (_, outsiderToken, _) = await outsiderClient.RegisterAndLoginAsync();
        outsiderClient.AuthenticateAs(outsiderToken);

        // Act
        var response = await outsiderClient.GetAsync($"/api/project/{project!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProject_ProjectDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var (_, token, _) = await Client.RegisterAndLoginAsync();
        Client.AuthenticateAs(token);

        // Act
        var response = await Client.GetAsync($"/api/project/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
