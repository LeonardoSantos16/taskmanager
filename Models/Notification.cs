using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace taskmanager.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TaskItemId { get; set; }
        public string? Message { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime Created { get; set; }
        [ForeignKey(nameof(TaskItemId))]
        [JsonIgnore]
        public TaskItem? TaskItem { get; set; }
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User? User { get; set; }

    }
}
