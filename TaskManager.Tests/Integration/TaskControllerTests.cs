using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using taskmanager.DTOs;
using taskmanager.Models;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Integration;

public class TaskControllerTests : IntegrationTestBase
{
    public TaskControllerTests(MsSqlContainerFixture sqlFixture) : base(sqlFixture)
    {
    }

    private static object BuildProjectRequest(string? name = null) => new
    {
        Name = name ?? $"Project-{Guid.NewGuid():N}"[..20],
        Description = (string?)null,
        Status = EnumProjectStatus.Active,
        OwnerId = Guid.Empty
    };

    private static object BuildTaskRequest(Guid projectId, Guid? assignedToId = null) => new
    {
        Title = "Integration test task",
        Description = (string?)null,
        DueDate = DateTime.UtcNow.AddDays(3),
        Priority = (EnumPriority?)null,
        AssignedToId = assignedToId,
        ProjectId = projectId,
        Status = EnumStatusTask.ToDo
    };

    private async Task<ProjectDtoResponse> CreateProjectAsOwnerAsync()
    {
        var (_, ownerToken, _) = await Client.RegisterAndLoginAsync();
        Client.AuthenticateAs(ownerToken);

        var createResponse = await Client.PostAsJsonAsync("/api/project", BuildProjectRequest());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await createResponse.Content.ReadFromJsonAsync<ProjectDtoResponse>())!;
    }

    private async Task<HttpClient> AddMemberAsync(Guid projectId, EnumRole role)
    {
        var memberClient = CreateAdditionalClient();
        var (memberId, memberToken, _) = await memberClient.RegisterAndLoginAsync();
        memberClient.AuthenticateAs(memberToken);

        var addMemberResponse = await Client.PostAsJsonAsync(
            $"/api/projectmember/project/{projectId}", new { UserId = memberId, Role = role });
        addMemberResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return memberClient;
    }

    [Fact]
    public async Task ChangeStatus_UserIsViewerMember_ReturnsForbidden()
    {
        // Arrange
        var project = await CreateProjectAsOwnerAsync();
        var createTaskResponse = await Client.PostAsJsonAsync($"/api/task/{project.Id}", BuildTaskRequest(project.Id));
        createTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = (await createTaskResponse.Content.ReadFromJsonAsync<TaskItemDtoResponse>())!;

        using var viewerClient = await AddMemberAsync(project.Id, EnumRole.Viewer);

        // Act
        var response = await viewerClient.PatchAsJsonAsync($"/api/task/{task.Id}/status", EnumStatusTask.InProgress);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeStatus_UserIsAssigneeButNotEditor_Succeeds()
    {
        // Arrange
        var project = await CreateProjectAsOwnerAsync();

        using var assigneeClient = CreateAdditionalClient();
        var (assigneeId, assigneeToken, _) = await assigneeClient.RegisterAndLoginAsync();
        assigneeClient.AuthenticateAs(assigneeToken);
        var addMemberResponse = await Client.PostAsJsonAsync(
            $"/api/projectmember/project/{project.Id}", new { UserId = assigneeId, Role = EnumRole.Viewer });
        addMemberResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createTaskResponse = await Client.PostAsJsonAsync(
            $"/api/task/{project.Id}", BuildTaskRequest(project.Id, assignedToId: assigneeId));
        createTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = (await createTaskResponse.Content.ReadFromJsonAsync<TaskItemDtoResponse>())!;

        // Act
        var response = await assigneeClient.PatchAsJsonAsync($"/api/task/{task.Id}/status", EnumStatusTask.Done);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await Client.GetAsync($"/api/task/{task.Id}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDtoResponse>();
        updatedTask!.Status.Should().Be(EnumStatusTask.Done);
    }

    [Fact]
    public async Task CreateTask_ProjectArchived_ReturnsConflict()
    {
        // Arrange
        var project = await CreateProjectAsOwnerAsync();
        var archiveResponse = await Client.PatchAsJsonAsync(
            $"/api/project/{project.Id}/status", new { NewStatus = EnumProjectStatus.Archived });
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/task/{project.Id}", BuildTaskRequest(project.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
