using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using taskmanager.Authorization;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Authorization;

public class CommentEditAuthorizationHandlerTests
{
    private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock = new();
    private readonly Mock<IProjectMemberRepository> _memberRepositoryMock = new();
    private readonly CommentEditAuthorizationHandler _sut;

    public CommentEditAuthorizationHandlerTests()
    {
        _sut = new CommentEditAuthorizationHandler(_taskItemRepositoryMock.Object, _memberRepositoryMock.Object);
    }

    private static AuthorizationHandlerContext BuildContext(Guid userId, TaskComment comment)
    {
        var user = EntityBuilders.CreateClaimsPrincipal(userId);
        return new AuthorizationHandlerContext(new[] { new CommentEditRequirement() }, user, comment);
    }

    private static TaskComment CreateComment(Guid taskItemId, Guid authorId) => new()
    {
        Id = Guid.NewGuid(),
        TaskItemId = taskItemId,
        AuthorId = authorId,
        Content = "Test comment",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task HandleRequirementAsync_UserIsCommentAuthor_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comment = CreateComment(Guid.NewGuid(), authorId: userId);
        var context = BuildContext(userId, comment);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        _taskItemRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserIsProjectOwnerButNotAuthor_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        var comment = CreateComment(task.Id, authorId: Guid.NewGuid());
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var membership = EntityBuilders.CreateProjectMember(projectId: task.ProjectId, userId: userId, role: EnumRole.Owner);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, comment);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumRole.Editor)]
    [InlineData(EnumRole.Viewer)]
    public async Task HandleRequirementAsync_UserIsMemberButNotOwnerNorAuthor_DoesNotSucceed(EnumRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        var comment = CreateComment(task.Id, authorId: Guid.NewGuid());
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var membership = EntityBuilders.CreateProjectMember(projectId: task.ProjectId, userId: userId, role: role);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, comment);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserNotMemberNorAuthor_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = EntityBuilders.CreateTaskItem();
        var comment = CreateComment(task.Id, authorId: Guid.NewGuid());
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(task.ProjectId, userId))
            .ReturnsAsync((ProjectMember?)null);
        var context = BuildContext(userId, comment);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_TaskNotFound_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comment = CreateComment(Guid.NewGuid(), authorId: Guid.NewGuid());
        _taskItemRepositoryMock.Setup(r => r.GetByIdAsync(comment.TaskItemId)).ReturnsAsync((TaskItem?)null);
        var context = BuildContext(userId, comment);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        _memberRepositoryMock.Verify(r => r.GetMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
