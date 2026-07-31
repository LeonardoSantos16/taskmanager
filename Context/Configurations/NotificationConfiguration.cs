using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using taskmanager.Models;

namespace taskmanager.Context.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder
               .HasOne(n => n.User)
               .WithMany(u => u.Notifications)
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(n => n.TaskItem)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
