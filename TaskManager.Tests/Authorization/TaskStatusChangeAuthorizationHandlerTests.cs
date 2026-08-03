using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using taskmanager.Authorization;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Authorization;

public class TaskStatusChangeAuthorizationHandlerTests
{
    private readonly Mock<IProjectMemberRepository> _memberRepositoryMock = new();
    private readonly TaskStatusChangeAuthorizationHandler _sut;

    public TaskStatusChangeAuthorizationHandlerTests()
    {
        _sut = new TaskStatusChangeAuthorizationHandler(_memberRepositoryMock.Object);
    }

    private static AuthorizationHandlerContext BuildContext(Guid userId, TaskItem task)
    {
        var user = EntityBuilders.CreateClaimsPrincipal(userId);
        return new AuthorizationHandlerContext(new[] { new TaskStatusChangeRequirement() }, user, task);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserIsCreator_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem(createdById: userId);
        var context = BuildContext(userId, task);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        _memberRepositoryMock.Verify(r => r.GetMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserIsAssignee_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem(assignedToId: userId);
        var context = BuildContext(userId, task);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumRole.Owner)]
    [InlineData(EnumRole.Editor)]
    public async Task HandleRequirementAsync_UserIsEditorOrOwnerMember_Succeeds(EnumRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        var membership = EntityBuilders.CreateProjectMember(projectId: task.ProjectId, userId: userId, role: role);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, task);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserIsViewerMember_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        var membership = EntityBuilders.CreateProjectMember(projectId: task.ProjectId, userId: userId, role: EnumRole.Viewer);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, task);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserNotMemberNorCreatorNorAssignee_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync((ProjectMember?)null);
        var context = BuildContext(userId, task);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
