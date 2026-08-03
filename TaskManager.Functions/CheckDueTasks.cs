using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using taskmanager.Models;
using taskmanager.Repositories;
using TaskManager.Functions.Notifications;
using TaskManager_Functions.DTOs;
using TaskManager_Functions.Notifications;

namespace TaskManager.Functions;

public class CheckDueTasks
{
    private readonly ILogger<CheckDueTasks> _logger;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IDueSoonEmailSender _emailSender;

    public CheckDueTasks(
        ILoggerFactory loggerFactory,
        ITaskItemRepository taskItemRepository,
        INotificationRepository notificationRepository,
        IDueSoonEmailSender emailSender)
    {
        _logger = loggerFactory.CreateLogger<CheckDueTasks>();
        _taskItemRepository = taskItemRepository;
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
    }

    [Function("CheckDueTasks")]
    public async Task Run([TimerTrigger("0 0 */12 * * *")] TimerInfo myTimer)
    {
        var now = DateTime.UtcNow;
        var dueSoonTasks = (await GetDueSoonTasksAsync(now, now.AddHours(24))).ToList();
        Console.WriteLine($"[DEBUG] dueSoonTasks.Count = {dueSoonTasks.Count}");

        if (dueSoonTasks.Count == 0)
        {
            _logger.LogInformation("No tasks due within the next 24 hours.");
            return;
        }

        var notifiedTaskIds = new List<Guid>();

        foreach (var task in dueSoonTasks)
        {
            var recipients = BuildNotificationData(task).ToList();
            Console.WriteLine($"[DEBUG] Task {task.Id} '{task.Title}': {recipients.Count} recipient(s) -> {string.Join(", ", recipients.Select(r => r.Email))}");

            foreach (var recipient in recipients)
            {
                try
                {
                    await _emailSender.SendDueSoonEmailAsync(
                        recipient.Email, recipient.Name, task.Title, task.Project!.Name, task.DueDate);
                    Console.WriteLine($"[DEBUG] SendDueSoonEmailAsync returned without throwing for {recipient.Email}");

                    await _notificationRepository.CreateAsync(new Notification
                    {
                        UserId = recipient.UserId,
                        TaskItemId = task.Id,
                        Message = recipient.Message,
                        Created = now
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Exception sending to {recipient.Email}: {ex}");
                    _logger.LogError(ex, "Failed to send due-soon email to {Email} for task {TaskId}", recipient.Email, task.Id);
                }


            }

            notifiedTaskIds.Add(task.Id);
        }

        var updated = await _taskItemRepository.MarkDueSoonNotifiedAsync(notifiedTaskIds, now);
        _logger.LogInformation("Processed {TaskCount} due-soon tasks, marked {UpdatedCount} as notified.", dueSoonTasks.Count, updated);
    }

    private Task<IEnumerable<TaskItem>> GetDueSoonTasksAsync(DateTime fromUtc, DateTime toUtc)
        => _taskItemRepository.GetDueSoonAsync(fromUtc, toUtc);

    private static IEnumerable<NotificationRecipient> BuildNotificationData(TaskItem task)
    {
        var owner = task.Project?.User;
        var assignee = task.UserAssigned;
        var message = $"A tarefa '{task.Title}' do projeto '{task.Project?.Name}' vence em menos de 24 horas.";
        var recipients = new List<NotificationRecipient>();

        if (owner is not null && !string.IsNullOrEmpty(owner.Email))
        {
            recipients.Add(new NotificationRecipient(owner.Id, owner.Email, owner.Name, message));
        }

        if (assignee is not null && assignee.Id != owner?.Id && !string.IsNullOrEmpty(assignee.Email))
        {
            recipients.Add(new NotificationRecipient(assignee.Id, assignee.Email, assignee.Name, message));
        }

        return recipients;
    }

}
