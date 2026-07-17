using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace taskmanager.Models
{
    [Table("ProjectMember")]
    public class ProjectMember
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required Guid ProjectId { get; set; }
        [Required]
        public required Guid UserId { get; set; }
        [Required]
        public required EnumRole Role { get; set; }
        [Required]
        public required DateTime JoinedAt { get; set; }
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User? User {get; set;}
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public Project? Project {get; set;}
    }

    public enum EnumRole
    {
        Owner,
        Editor,
        Viewer
    }
}