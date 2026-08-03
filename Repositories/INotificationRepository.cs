using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly);
    }
}
