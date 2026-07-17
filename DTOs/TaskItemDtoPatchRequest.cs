using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record TaskItemDtoPatchRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public DateTime? DueDate { get; init; }
        public EnumPriority? Priority { get; init; }
        public Guid? AssignedToId { get; init; }
        public EnumStatusTask? Status { get; init; } 
    }
}