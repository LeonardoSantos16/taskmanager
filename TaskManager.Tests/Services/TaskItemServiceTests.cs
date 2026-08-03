using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using taskmanager.DTOs;
using taskmanager.Models;
using taskmanager.Repositories;
using taskmanager.Services;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Services;

public class TaskItemServiceTests
{
    private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock = new();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IProjectMemberService> _projectMemberServiceMock = new();
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly TaskItemService _sut;

    public TaskItemServiceTests()
    {
        _sut = new TaskItemService(
            _taskItemRepositoryMock.Object,
            _projectServiceMock.Object,
            _projectMemberServiceMock.Object,
            _authorizationServiceMock.Object);

        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
    }

    private void DenyStatusChangeAuthorization()
    {
        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());
    }

    // RN10 / RN11 — CompletedAt follows Status

    [Fact]
    public async Task ChangeTaskItemStatusAsync_ToDone_SetsCompletedAt()
    {
        var task = EntityBuilders.CreateTaskItem(status: EnumStatusTask.InProgress);
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        _taskItemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        await _sut.ChangeTaskItemStatusAsync(task.Id, EnumStatusTask.Done, EntityBuilders.CreateClaimsPrincipal(task.CreatedById));

        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeTaskItemStatusAsync_FromDoneToOther_ClearsCompletedAt()
    {
        var task = EntityBuilders.CreateTaskItem(status: EnumStatusTask.Done);
        task.CompletedAt = DateTime.UtcNow;
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        _taskItemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        await _sut.ChangeTaskItemStatusAsync(task.Id, EnumStatusTask.InProgress, EntityBuilders.CreateClaimsPrincipal(task.CreatedById));

        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task ChangeTaskItemStatusAsync_TaskNotFound_ThrowsArgumentException()
    {
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        var act = async () => await _sut.ChangeTaskItemStatusAsync(
            Guid.NewGuid(), EnumStatusTask.Done, EntityBuilders.CreateClaimsPrincipal(Guid.NewGuid()));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ChangeTaskItemStatusAsync_UserWithoutPermission_ThrowsUnauthorizedAccessException()
    {
        var task = EntityBuilders.CreateTaskItem();
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        DenyStatusChangeAuthorization();

        var act = async () => await _sut.ChangeTaskItemStatusAsync(
            task.Id, EnumStatusTask.Done, EntityBuilders.CreateClaimsPrincipal(Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    // RN13 — DueDate cannot be earlier than (or equal to) the reference creation date

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateDueDate_DueDateNotAfterCreatedDate_ThrowsArgumentException(int daysOffset)
    {
        var createdAt = DateTime.UtcNow;
        var dueDate = createdAt.AddDays(daysOffset);

        var act = () => _sut.ValidateDueDate(dueDate, createdAt);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateDueDate_DueDateAfterCreatedDate_DoesNotThrow()
    {
        var createdAt = DateTime.UtcNow;
        var dueDate = createdAt.AddDays(1);

        var act = () => _sut.ValidateDueDate(dueDate, createdAt);

        act.Should().NotThrow();
    }

    // RN09 — assignee must be a member of the project

    [Fact]
    public async Task EnsureAssigneeIsProjectMemberAsync_UserNotMember_ThrowsArgumentException()
    {
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        _projectMemberServiceMock.Setup(s => s.IsMemberAsync(projectId, assigneeId)).ReturnsAsync(false);

        var act = async () => await _sut.EnsureAssigneeIsProjectMemberAsync(projectId, assigneeId);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnsureAssigneeIsProjectMemberAsync_UserIsMember_DoesNotThrow()
    {
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        _projectMemberServiceMock.Setup(s => s.IsMemberAsync(projectId, assigneeId)).ReturnsAsync(true);

        var act = async () => await _sut.EnsureAssigneeIsProjectMemberAsync(projectId, assigneeId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateTaskItemAsync_AssigneeNotProjectMember_ThrowsArgumentException()
    {
        var project = EntityBuilders.CreateProject();
        _projectServiceMock.Setup(s => s.GetProjectEntityByIdAsync(project.Id)).ReturnsAsync(project);
        _projectMemberServiceMock.Setup(s => s.IsMemberAsync(project.Id, It.IsAny<Guid>())).ReturnsAsync(false);

        var dto = new TaskItemDtoRequest
        {
            Title = "New task",
            DueDate = DateTime.UtcNow.AddDays(3),
            ProjectId = project.Id,
            AssignedToId = Guid.NewGuid(),
            Status = EnumStatusTask.ToDo
        };

        var act = async () => await _sut.CreateTaskItemAsync(dto, project.Id, EntityBuilders.CreateClaimsPrincipal(project.OwnerId));

        await act.Should().ThrowAsync<ArgumentException>();
        _taskItemRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task CreateTaskItemAsync_ValidData_CreatesTaskAndSetsCreatedBy()
    {
        var project = EntityBuilders.CreateProject();
        var currentUserId = Guid.NewGuid();
        _projectServiceMock.Setup(s => s.GetProjectEntityByIdAsync(project.Id)).ReturnsAsync(project);
        _taskItemRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        var dto = new TaskItemDtoRequest
        {
            Title = "New task",
            DueDate = DateTime.UtcNow.AddDays(3),
            ProjectId = project.Id,
            Status = EnumStatusTask.ToDo
        };

        var result = await _sut.CreateTaskItemAsync(dto, project.Id, EntityBuilders.CreateClaimsPrincipal(currentUserId));

        result.Title.Should().Be(dto.Title);
        _taskItemRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<TaskItem>(t => t.CreatedById == currentUserId && t.ProjectId == project.Id)),
            Times.Once);
    }
}
