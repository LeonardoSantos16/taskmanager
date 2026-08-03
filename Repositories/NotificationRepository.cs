using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => n.ReadAt == null);

            return await query
                .Include(n => n.TaskItem)
                .Include(n => n.User)
                .OrderByDescending(n => n.Created)
                .ToListAsync();
        }
    }
}
