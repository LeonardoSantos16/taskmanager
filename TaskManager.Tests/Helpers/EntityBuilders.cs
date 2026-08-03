using System.Security.Claims;
using taskmanager.Models;

namespace TaskManager.Tests.Helpers;

public static class EntityBuilders
{
    public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, string? email = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (email is not null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }


    public static User CreateUser(
        Guid? id = null,
        string? name = null,
        string? email = null)
    {
        var userId = id ?? Guid.NewGuid();
        return new User
        {
            Id = userId,
            Name = name ?? $"User {userId}",
            UserName = email ?? $"user-{userId}@example.com",
            Email = email ?? $"user-{userId}@example.com"
        };
    }

    public static Project CreateProject(
        Guid? id = null,
        string? name = null,
        EnumProjectStatus status = EnumProjectStatus.Active,
        Guid? ownerId = null)
    {
        return new Project
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? "Test Project",
            Status = status,
            OwnerId = ownerId ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ProjectMember CreateProjectMember(
        Guid? id = null,
        Guid? projectId = null,
        Guid? userId = null,
        EnumRole role = EnumRole.Editor)
    {
        return new ProjectMember
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = projectId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Role = role,
            JoinedAt = DateTime.UtcNow
        };
    }

    public static TaskItem CreateTaskItem(
        Guid? id = null,
        Guid? projectId = null,
        string? title = null,
        EnumStatusTask status = EnumStatusTask.ToDo,
        EnumPriority? priority = null,
        Guid? assignedToId = null,
        Guid? createdById = null,
        DateTime? dueDate = null)
    {
        var createdAt = DateTime.UtcNow;
        return new TaskItem
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = projectId ?? Guid.NewGuid(),
            Title = title ?? "Test Task",
            Status = status,
            Priority = priority,
            AssignedToId = assignedToId,
            CreatedById = createdById ?? Guid.NewGuid(),
            DueDate = dueDate ?? createdAt.AddDays(7),
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
