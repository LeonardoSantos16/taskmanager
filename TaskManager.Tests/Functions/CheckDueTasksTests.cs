using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Functions;
using TaskManager.Tests.Helpers;
using TaskManager_Functions.Notifications;

namespace TaskManager.Tests.Functions;

public class CheckDueTasksTests
{
    private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock = new();
    private readonly Mock<INotificationRepository> _notificationRepositoryMock = new();
    private readonly Mock<IDueSoonEmailSender> _emailSenderMock = new();
    private readonly CheckDueTasks _sut;

    public CheckDueTasksTests()
    {
        _sut = new CheckDueTasks(
            LoggerFactory.Create(builder => { }),
            _taskItemRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _emailSenderMock.Object);

        _emailSenderMock
            .Setup(s => s.SendDueSoonEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        _taskItemRepositoryMock
            .Setup(r => r.MarkDueSoonNotifiedAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>()))
            .ReturnsAsync((IEnumerable<Guid> ids, DateTime _) => ids.Count());
    }

    private static TimerInfo CreateTimerInfo() => new();

    private void SetupDueSoonTasks(params TaskItem[] tasks)
    {
        _taskItemRepositoryMock
            .Setup(r => r.GetDueSoonAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(tasks);
    }

    private static TaskItem CreateTaskWithRecipients(User? owner, User? assignee)
    {
        var project = EntityBuilders.CreateProject(ownerId: owner?.Id ?? Guid.NewGuid());
        project.User = owner;

        var task = EntityBuilders.CreateTaskItem(projectId: project.Id, assignedToId: assignee?.Id);
        task.Project = project;
        task.UserAssigned = assignee;

        return task;
    }

    [Fact]
    public async Task Run_NoDueSoonTasks_DoesNotSendEmailOrMarkNotified()
    {
        SetupDueSoonTasks();

        await _sut.Run(CreateTimerInfo());

        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _taskItemRepositoryMock.Verify(r => r.MarkDueSoonNotifiedAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Run_OwnerAndAssigneeAreTheSameUser_SendsOnlyOneEmail()
    {
        var owner = EntityBuilders.CreateUser(name: "Owner", email: "owner@example.com");
        var task = CreateTaskWithRecipients(owner, owner);
        SetupDueSoonTasks(task);

        await _sut.Run(CreateTimerInfo());

        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_OwnerAndAssigneeAreDifferentUsers_SendsEmailToBoth()
    {
        var owner = EntityBuilders.CreateUser(name: "Owner", email: "owner@example.com");
        var assignee = EntityBuilders.CreateUser(name: "Assignee", email: "assignee@example.com");
        var task = CreateTaskWithRecipients(owner, assignee);
        SetupDueSoonTasks(task);

        await _sut.Run(CreateTimerInfo());

        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(owner.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(assignee.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_SuccessfulSend_CreatesNotificationForRecipient()
    {
        var owner = EntityBuilders.CreateUser(name: "Owner", email: "owner@example.com");
        var task = CreateTaskWithRecipients(owner, assignee: null);
        SetupDueSoonTasks(task);

        await _sut.Run(CreateTimerInfo());

        _notificationRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Notification>(n => n.UserId == owner.Id && n.TaskItemId == task.Id)),
            Times.Once);
    }

    [Fact]
    public async Task Run_EmailSendFailsForOneRecipient_StillProcessesTheOtherRecipient()
    {
        var owner = EntityBuilders.CreateUser(name: "Owner", email: "owner@example.com");
        var assignee = EntityBuilders.CreateUser(name: "Assignee", email: "assignee@example.com");
        var task = CreateTaskWithRecipients(owner, assignee);
        SetupDueSoonTasks(task);

        _emailSenderMock
            .Setup(s => s.SendDueSoonEmailAsync(owner.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SendGrid unavailable"));

        await _sut.Run(CreateTimerInfo());

        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(assignee.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Notification>(n => n.UserId == owner.Id)),
            Times.Never);
        _notificationRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Notification>(n => n.UserId == assignee.Id)),
            Times.Once);
    }

    [Fact]
    public async Task Run_ProcessesAllDueSoonTasks_MarksAllOfThemAsNotified()
    {
        var owner1 = EntityBuilders.CreateUser(email: "owner1@example.com");
        var owner2 = EntityBuilders.CreateUser(email: "owner2@example.com");
        var task1 = CreateTaskWithRecipients(owner1, assignee: null);
        var task2 = CreateTaskWithRecipients(owner2, assignee: null);
        SetupDueSoonTasks(task1, task2);

        await _sut.Run(CreateTimerInfo());

        _taskItemRepositoryMock.Verify(
            r => r.MarkDueSoonNotifiedAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(task1.Id) && ids.Contains(task2.Id) && ids.Count() == 2),
                It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_TaskWithNoRecipients_StillMarksTaskAsNotified()
    {
        var task = CreateTaskWithRecipients(owner: null, assignee: null);
        SetupDueSoonTasks(task);

        await _sut.Run(CreateTimerInfo());

        _emailSenderMock.Verify(
            s => s.SendDueSoonEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _taskItemRepositoryMock.Verify(
            r => r.MarkDueSoonNotifiedAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(task.Id)), It.IsAny<DateTime>()),
            Times.Once);
    }
}
