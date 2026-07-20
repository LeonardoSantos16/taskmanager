using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace taskmanager.Models
{
    [Table("Project")]
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public required EnumProjectStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [Required]
        public required Guid OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public User? User {get; set;}
        public ICollection<ProjectMember> ProjectMemberships {get; set;} = new List<ProjectMember>();
        public ICollection<TaskItem> Tasks {get; set;} = new List<TaskItem>();
    }


}