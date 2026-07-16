using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace taskmanager.Models
{
    [Table("TaskItem")]
    public class TaskItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required Guid ProjectId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public required EnumStatusTask Status { get; set; }
        public EnumPriority? Priority { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public required Project Project {get; set;}
        [ForeignKey(nameof(AssignedToId))]
        [JsonIgnore]
        public User? UserAssigned {get; set;}
        [ForeignKey(nameof(CreatedById))]
        [JsonIgnore]
        public required User UserCreated {get; set;}
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }

}