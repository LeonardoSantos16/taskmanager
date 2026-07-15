using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace taskmanager.Models
{
    [Table("TaskComment")]
    public class TaskComment
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid TaskItemId { get; set; }
        [Required]
        public Guid AuthorId { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Comment cannot be longer than 200 characters.")]
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey(nameof(TaskItemId))]
        [JsonIgnore]
        public required TaskItem TaskItem { get; set;}
        [ForeignKey(nameof(AuthorId))]
        [JsonIgnore]
        public required User User {get; set;}

    }
}