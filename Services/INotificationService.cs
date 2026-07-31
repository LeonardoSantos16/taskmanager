using taskmanager.DTOs;

namespace taskmanager.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDtoResponse>> GetMyNotificationsAsync(Guid userId, bool unreadOnly);
        Task<NotificationDtoResponse> MarkAsReadAsync(Guid notificationId, Guid userId);
    }
}
