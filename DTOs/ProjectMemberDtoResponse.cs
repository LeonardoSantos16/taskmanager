using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public class ProjectMemberDtoResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public EnumRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}