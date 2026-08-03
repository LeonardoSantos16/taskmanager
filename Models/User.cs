using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.Models
{
    [Table("Users")]
    public class User : IdentityUser<Guid>
    {
        [Required]
        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
        public required string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public ICollection<Project> Projects {get; set;} = new List<Project>();
        public ICollection<ProjectMember> ProjectMemberships {get; set;} = new List<ProjectMember>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<TaskComment> Comments {get; set;} = new List<TaskComment>();
        public ICollection<TaskItem> TasksAssigned { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> TasksCreated { get; set; } = new List<TaskItem>();
    }
}