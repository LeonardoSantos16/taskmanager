using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using taskmanager.Authorization;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Authorization;

public class ProjectRoleAuthorizationHandlerTests
{
    private readonly Mock<IProjectMemberRepository> _memberRepositoryMock = new();
    private readonly ProjectRoleAuthorizationHandler _sut;

    public ProjectRoleAuthorizationHandlerTests()
    {
        _sut = new ProjectRoleAuthorizationHandler(_memberRepositoryMock.Object);
    }

    private static AuthorizationHandlerContext BuildContext(
        Guid userId, Guid projectId, ProjectRoleRequirement requirement)
    {
        var user = EntityBuilders.CreateClaimsPrincipal(userId);
        return new AuthorizationHandlerContext(new[] { requirement }, user, projectId);
    }

    [Theory]
    [InlineData(EnumRole.Owner)]
    [InlineData(EnumRole.Editor)]
    public async Task HandleRequirementAsync_UserHasAllowedRole_Succeeds(EnumRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var requirement = new ProjectRoleRequirement(EnumRole.Owner, EnumRole.Editor);
        var membership = EntityBuilders.CreateProjectMember(projectId: projectId, userId: userId, role: role);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(projectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, projectId, requirement);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserRoleNotAllowed_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var requirement = new ProjectRoleRequirement(EnumRole.Owner, EnumRole.Editor);
        var membership = EntityBuilders.CreateProjectMember(projectId: projectId, userId: userId, role: EnumRole.Viewer);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(projectId, userId))
            .ReturnsAsync(membership);
        var context = BuildContext(userId, projectId, requirement);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserNotMember_DoesNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var requirement = new ProjectRoleRequirement(EnumRole.Owner, EnumRole.Editor, EnumRole.Viewer);
        _memberRepositoryMock
            .Setup(r => r.GetMembershipAsync(projectId, userId))
            .ReturnsAsync((ProjectMember?)null);
        var context = BuildContext(userId, projectId, requirement);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
