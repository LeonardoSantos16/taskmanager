using taskmanager.Models;

namespace taskmanager.DTOs.Mappings
{
    public static class NotificationMappingExtensions
    {
        public static NotificationDtoResponse ToDtoResponse(this Notification notification)
        {
            return new NotificationDtoResponse
            {
                Id = notification.Id,
                Message = notification.Message,
                TaskItemId = notification.TaskItemId,
                CreatedAt = notification.Created,
                ReadAt = notification.ReadAt
            };
        }

        public static IEnumerable<NotificationDtoResponse> ToDtoResponse(this IEnumerable<Notification> notifications)
        {
            return notifications.Select(n => n.ToDtoResponse());
        }
    }
}
