using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Repositories;

[Collection(MsSqlCollection.Name)]
public class TaskItemRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;
    private AppDbContext _context = null!;
    private TaskItemRepository _repository = null!;

    public TaskItemRepositoryTests(MsSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_fixture.Container.GetConnectionString())
        {
            InitialCatalog = $"TaskManagerTests_{Guid.NewGuid():N}"
        };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionStringBuilder.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.MigrateAsync();
        _repository = new TaskItemRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private async Task<(Project project, User owner)> SeedProjectAsync()
    {
        var owner = EntityBuilders.CreateUser();
        var project = EntityBuilders.CreateProject(ownerId: owner.Id);
        _context.Users.Add(owner);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return (project, owner);
    }

    private async Task<TaskItem> SeedTaskAsync(
        Guid projectId,
        Guid createdById,
        EnumStatusTask status = EnumStatusTask.ToDo,
        DateTime? dueDate = null,
        DateTime? dueSoonNotifiedAt = null,
        Guid? assignedToId = null)
    {
        var task = EntityBuilders.CreateTaskItem(
            projectId: projectId,
            createdById: createdById,
            status: status,
            dueDate: dueDate,
            assignedToId: assignedToId);
        task.DueSoonNotifiedAt = dueSoonNotifiedAt;

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    [Fact]
    public async Task GetDueSoonAsync_TaskWithinWindowAndNotNotified_ReturnsTask()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        var task = await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12));

        // Act
        var result = await _repository.GetDueSoonAsync(now, now.AddHours(24));

        // Assert
        result.Should().ContainSingle(t => t.Id == task.Id);
    }

    [Fact]
    public async Task GetDueSoonAsync_TaskAlreadyNotified_IsExcluded()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12), dueSoonNotifiedAt: now.AddHours(-1));

        // Act
        var result = await _repository.GetDueSoonAsync(now, now.AddHours(24));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDueSoonAsync_TaskOutsideWindow_IsExcluded()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddDays(3));

        // Act
        var result = await _repository.GetDueSoonAsync(now, now.AddHours(24));

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(EnumStatusTask.Done)]
    [InlineData(EnumStatusTask.Canceled)]
    public async Task GetDueSoonAsync_TaskWithDoneOrCancelledStatus_IsExcluded(EnumStatusTask status)
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        await SeedTaskAsync(project.Id, owner.Id, status: status, dueDate: now.AddHours(12));

        // Act
        var result = await _repository.GetDueSoonAsync(now, now.AddHours(24));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDueSoonAsync_IncludesProjectOwnerAndAssignedUser()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var assignee = EntityBuilders.CreateUser();
        _context.Users.Add(assignee);
        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var task = await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12), assignedToId: assignee.Id);

        // Act
        var result = await _repository.GetDueSoonAsync(now, now.AddHours(24));

        // Assert
        var returned = result.Should().ContainSingle(t => t.Id == task.Id).Subject;
        returned.Project.Should().NotBeNull();
        returned.Project!.User.Should().NotBeNull();
        returned.Project.User!.Id.Should().Be(owner.Id);
        returned.UserAssigned.Should().NotBeNull();
        returned.UserAssigned!.Id.Should().Be(assignee.Id);
    }

    [Fact]
    public async Task MarkDueSoonNotifiedAsync_SetsDueSoonNotifiedAtForGivenTasks()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        var task = await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12));

        // Act
        var updatedCount = await _repository.MarkDueSoonNotifiedAsync(new[] { task.Id }, now);

        // Assert
        updatedCount.Should().Be(1);
        var reloaded = await _context.TaskItems.AsNoTracking().FirstAsync(t => t.Id == task.Id);
        reloaded.DueSoonNotifiedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MarkDueSoonNotifiedAsync_DoesNotAffectTasksNotInList()
    {
        // Arrange
        var (project, owner) = await SeedProjectAsync();
        var now = DateTime.UtcNow;
        var taskToMark = await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12));
        var taskToLeaveAlone = await SeedTaskAsync(project.Id, owner.Id, dueDate: now.AddHours(12));

        // Act
        await _repository.MarkDueSoonNotifiedAsync(new[] { taskToMark.Id }, now);

        // Assert
        var reloaded = await _context.TaskItems.AsNoTracking().FirstAsync(t => t.Id == taskToLeaveAlone.Id);
        reloaded.DueSoonNotifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkDueSoonNotifiedAsync_EmptyIdList_ReturnsZeroAndDoesNotThrow()
    {
        // Act
        var updatedCount = await _repository.MarkDueSoonNotifiedAsync(Array.Empty<Guid>(), DateTime.UtcNow);

        // Assert
        updatedCount.Should().Be(0);
    }
}
