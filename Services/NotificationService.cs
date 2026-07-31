using taskmanager.DTOs;
using taskmanager.DTOs.Mappings;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<NotificationDtoResponse>> GetMyNotificationsAsync(Guid userId, bool unreadOnly)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, unreadOnly);
            return notifications.ToDtoResponse();
        }

        public async Task<NotificationDtoResponse> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId)
                ?? throw new ArgumentException("Notification not found.");

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this notification.");
            }

            if (notification.ReadAt is null)
            {
                notification.ReadAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);
            }

            return notification.ToDtoResponse();
        }
    }
}
